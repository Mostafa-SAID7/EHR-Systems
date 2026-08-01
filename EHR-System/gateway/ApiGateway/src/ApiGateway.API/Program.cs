using System.Text;
using System.Threading.RateLimiting;
using EHRPlatform.BuildingBlocks.Common.Application.Common.Extensions;
using EHRPlatform.BuildingBlocks.Common.Middleware;
using EHRPlatform.Services.ApiGateway.Infrastructure.Routing;
using EHRPlatform.Services.ApiGateway.Infrastructure.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;

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
    builder.Services.AddApiGatewayMetrics();

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

    builder.Services.AddAuthorization(options =>
    {
        options.AddPolicy("Bearer", policy => policy.RequireAuthenticatedUser());
    });

    // ── CORS ──────────────────────────────────────────────────────────────────
    builder.Services.AddCors(options =>
        options.AddPolicy("AllowAll", p =>
            p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

    // ── Rate limiting ─────────────────────────────────────────────────────────
    builder.Services.AddRateLimiter(options =>
    {
        options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
        {
            var userId = httpContext.User?.FindFirst("sub")?.Value;

            if (!string.IsNullOrEmpty(userId))
            {
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
    app.UseEHRGlobalExceptionHandler();
    app.UseApiGatewayMetrics();
    app.UseRequestTracking();
    app.UseSerilogRequestLogging();
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "EHR API Gateway v1");
        c.RoutePrefix = "swagger";
    });

    app.UseCors("AllowAll");
    app.UseRateLimiter();
    app.UseAuthentication();
    app.UseAuthorization();

    app.MapHealthChecks("/health");
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
