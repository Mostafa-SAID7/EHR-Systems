using System.Text;
using System.Threading.RateLimiting;
using EHRPlatform.Common.Extensions;
using EHRPlatform.Common.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using EHRPlatform.Services.ApiGateway.Middleware;

// ── Bootstrap logger ──────────────────────────────────────────────────────────
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .Enrich.FromLogContext()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    // ── Serilog ───────────────────────────────────────────────────────────────
    builder.Host.UseSerilog((ctx, cfg) =>
        cfg.MinimumLevel.Information()
           .WriteTo.Console()
           .Enrich.FromLogContext());

    // ── OpenTelemetry Metrics ─────────────────────────────────────────────────
    builder.Services.AddOpenTelemetryObservability("api-gateway");
    builder.Services.AddApiGatewayMetrics();  // ← Add gateway-specific metrics

    // ── YARP Reverse Proxy ────────────────────────────────────────────────────
    builder.Services.AddReverseProxy()
        .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

    // ── Redis cache (optional — rate limiting / session caching) ──────────────
    var redisConn = builder.Configuration.GetConnectionString("Redis")
                 ?? Environment.GetEnvironmentVariable("REDIS_CONNECTION_STRING");
    if (!string.IsNullOrEmpty(redisConn))
    {
        builder.Services.AddStackExchangeRedisCache(o => o.Configuration = redisConn);
        Log.Information("Redis caching enabled for API Gateway");
    }


    // ── Swagger / OpenAPI ─────────────────────────────────────────────────────
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new OpenApiInfo
        {
            Title       = "EHR API Gateway",
            Version     = "v1",
            Description = "Unified entry point for the EHR Platform microservices"
        });
        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Type          = SecuritySchemeType.Http,
            Scheme        = "bearer",
            BearerFormat  = "JWT",
            Description   = "JWT Authorization header using the Bearer scheme"
        });
        options.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id   = "Bearer"
                    }
                },
                Array.Empty<string>()
            }
        });
    });

    // ── JWT Authentication ────────────────────────────────────────────────────
    var jwtSecret = builder.Configuration["Jwt:Secret"]
                 ?? Environment.GetEnvironmentVariable("JWT_SECRET")
                 ?? throw new InvalidOperationException("JWT_SECRET is required");

    builder.Services
        .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
                ValidateIssuer           = true,
                ValidIssuer              = builder.Configuration["Jwt:Issuer"]   ?? "ehr-platform",
                ValidateAudience         = true,
                ValidAudience            = builder.Configuration["Jwt:Audience"] ?? "ehr-api",
                ValidateLifetime         = true
            };
        });

    // Register the "Bearer" authorization policy that YARP route configs reference.
    // Any route with AuthorizationPolicy:"Bearer" requires a valid authenticated user.
    builder.Services.AddAuthorization(options =>
    {
        options.AddPolicy("Bearer", policy => policy.RequireAuthenticatedUser());
    });

    // ── CORS ──────────────────────────────────────────────────────────────────
    builder.Services.AddCors(options =>
        options.AddPolicy("AllowAll", p =>
            p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

    // ── Rate limiting ─────────────────────────────────────────────────────────
    // GlobalLimiter applies to every request passing through the gateway.
    // Authenticated users get a per-user sliding window (100 req/min).
    // Anonymous / unauthenticated requests share a fixed window (200 req/min).
    builder.Services.AddRateLimiter(options =>
    {
        options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
        {
            var userId = httpContext.User?.FindFirst("sub")?.Value;

            if (!string.IsNullOrEmpty(userId))
            {
                // Per-authenticated-user sliding window
                return RateLimitPartition.GetSlidingWindowLimiter(userId, _ =>
                    new SlidingWindowRateLimiterOptions
                    {
                        PermitLimit          = 100,
                        Window               = TimeSpan.FromSeconds(60),
                        SegmentsPerWindow    = 4,
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit           = 10
                    });
            }

            // Anonymous: fixed window shared across all unauthenticated callers
            return RateLimitPartition.GetFixedWindowLimiter("anonymous", _ =>
                new FixedWindowRateLimiterOptions
                {
                    PermitLimit          = 200,
                    Window               = TimeSpan.FromSeconds(60),
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                    QueueLimit           = 0
                });
        });

        options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
        options.OnRejected = async (context, token) =>
        {
            context.HttpContext.Response.ContentType = "application/problem+json";
            await context.HttpContext.Response.WriteAsync(
                "{\"title\":\"Rate limit exceeded\",\"status\":429}", token);
        };
    });

    // ── Health checks ─────────────────────────────────────────────────────────
    builder.Services.AddHealthChecks();

    var app = builder.Build();

    // ── Middleware pipeline ───────────────────────────────────────────────────
    // 1. Global exception handler — outermost, catches everything
    app.UseEHRGlobalExceptionHandler();

    // 1.5. API Gateway Metrics — collect requests/latency/errors
    app.UseApiGatewayMetrics();  // ← Collect gateway metrics

    // 2. Request tracking — single source of truth for correlation ID in the gateway.
    //    Generates/propagates X-Correlation-ID, measures latency, scrubs PII from paths.
    //    Do NOT also call UseEHRCorrelationId() here: RequestTrackingMiddleware already
    //    handles the full correlation-ID lifecycle, so a second middleware would
    //    re-generate a new ID for requests that arrive with only X-Request-ID, breaking
    //    trace/log correlation across the pipeline.
    //    (UseEHRCorrelationId is for microservices that have no gateway-level tracking.)
    app.UseRequestTracking();

    // 4. Serilog structured request logging
    app.UseSerilogRequestLogging();

    // 5. Swagger (all environments — gateway is internal-facing)
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "EHR API Gateway v1");
        c.RoutePrefix = "swagger";
    });

    // 6. Security
    app.UseCors("AllowAll");
    app.UseRateLimiter();
    app.UseAuthentication();
    app.UseAuthorization();

    // 7. Health & proxy
    app.MapHealthChecks("/health");
    // Metrics endpoint (Prometheus scrape): disabled in favor of OTLP export
    // app.MapPrometheusMetricsEndpoint();
    app.MapReverseProxy();

    Log.Information("EHR API Gateway starting on port 5000");
    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "API Gateway terminated unexpectedly");
    throw;
}
finally
{
    Log.CloseAndFlush();
}
