using EHRPlatform.Gateway.Infrastructure.Middleware;
using EHRPlatform.Gateway.Infrastructure.Routing;
using EHRPlatform.Gateway.Infrastructure.Services;
using EHRPlatform.Gateway.Infrastructure.Observability;
using EHRPlatform.Gateway.Infrastructure.HealthChecks;
using EHRPlatform.Gateway.DTOs.Responses;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Text;
using Yarp.ReverseProxy.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Logging with Serilog
builder.Host.UseSerilog((context, config) =>
{
    config
        .MinimumLevel.Information()
        .WriteTo.Console()
        .WriteTo.File("logs/gateway-.txt", rollingInterval: RollingInterval.Day)
        .Enrich.FromLogContext()
        .Enrich.WithProperty("Application", "APIGateway");
});

// Services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// YARP Reverse Proxy
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

// Authentication
var jwtSettings = builder.Configuration.GetSection("Jwt");
var key = Encoding.UTF8.GetBytes(jwtSettings["SecretKey"] ?? "your-secret-key-here");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidateAudience = true,
            ValidAudience = jwtSettings["Audience"],
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };

        options.Events = new JwtBearerEvents
        {
            OnChallenge = async context =>
            {
                context.HandleResponse();
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsJsonAsync(new
                {
                    error = "Unauthorized",
                    message = "Missing or invalid JWT token"
                });
            }
        };
    });

// Authorization
builder.Services.AddAuthorization();

// Health Checks
builder.Services.AddHealthChecks()
    .AddCheck<ServiceHealthCheck>("identity-health", tags: new[] { "services", "identity" })
    .AddCheck<ServiceHealthCheck>("patient-health", tags: new[] { "services", "patient" })
    .AddCheck<ServiceHealthCheck>("appointment-health", tags: new[] { "services", "appointment" })
    .AddCheck<ServiceHealthCheck>("audit-health", tags: new[] { "services", "audit" });

// Rate Limiting
builder.Services.AddRateLimiter(rateLimiterOptions =>
{
    rateLimiterOptions.AddFixedWindowLimiter(policyName: "standard", options =>
    {
        options.PermitLimit = 100;
        options.Window = TimeSpan.FromMinutes(1);
        options.QueueProcessingOrder = System.Threading.RateLimiting.QueueProcessingOrder.OldestFirst;
    });
});

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("https://localhost:3000", "https://app.example.com")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// Gateway Services
builder.Services.AddSingleton<IServiceRegistry, ServiceRegistry>();
builder.Services.AddSingleton<IRequestTransformer, RequestTransformer>();
builder.Services.AddScoped<IResponseAggregator, ResponseAggregator>();

// Metrics (from building-blocks integration)
builder.Services.AddGatewayMetrics();

// HTTP Client Factory
builder.Services.AddHttpClient();

var app = builder.Build();

// Middleware Pipeline (Order Matters!)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// 1. Logging
app.UseSerilogRequestLogging();

// 2. Metrics
app.UseGatewayMetrics();

// 3. Correlation ID
app.UseMiddleware<CorrelationIdMiddleware>();

// 4. Global Exception Handler
app.UseMiddleware<GlobalExceptionMiddleware>();

// 5. HTTPS Redirection
app.UseHttpsRedirection();

// 6. CORS
app.UseCors("AllowFrontend");

// 7. Rate Limiting
app.UseRateLimiter();

// 8. Authentication
app.UseAuthentication();

// 9. Authorization
app.UseAuthorization();

// 10. Custom Middleware
app.UseMiddleware<RequestEnrichmentMiddleware>();
app.UseMiddleware<ResponseTransformMiddleware>();

// Routes
app.MapControllers();
app.MapHealthChecks("/health");
app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("services")
});

// YARP Routing
app.MapReverseProxy();

// Custom route mapping
app.MapGatewayRoutes();

app.Run();
