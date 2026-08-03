using EHRPlatform.BuildingBlocks.Common.Data;
using EHRPlatform.BuildingBlocks.Common.Application.Common.Extensions;
using EHRPlatform.BuildingBlocks.Observability.HealthChecks;
using EHRPlatform.BuildingBlocks.Common.Search;
using EHRPlatform.BuildingBlocks.Security.Authentication;
using EHRPlatform.BuildingBlocks.Security.Jwt;
using EHRPlatform.BuildingBlocks.Common.Data.Migrations;
using EHRPlatform.Services.Identity.Application.Identity.Extensions;
using EHRPlatform.Services.Identity.Data;
using EHRPlatform.Services.Identity.Persistence.Repositories;
using EHRPlatform.Services.Identity.Application.Services;
using EHRPlatform.Services.Identity.Infrastructure.Extensions;
using EHRPlatform.Services.Identity.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Serilog;
using StackExchange.Redis;

// Bootstrap logger to capture startup errors
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .Enrich.FromLogContext()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    // â”€â”€ Serilog â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    builder.Host.UseSerilog((ctx, cfg) =>
        cfg.MinimumLevel.Information()
           .WriteTo.Console()
           .Enrich.FromLogContext());

    // â”€â”€ OpenTelemetry Metrics â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    // Exposes /metrics endpoint for Prometheus scraping
    // Collects: HTTP metrics, runtime (GC, memory), process (CPU), ASP.NET Core
    builder.Services.AddOpenTelemetryObservability("identity-service");
    builder.Services.AddIdentityMetrics();  // â† Add identity-specific metrics

    // â”€â”€ Controllers & Swagger â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo { Title = "EHR Identity Service", Version = "v1" });
        c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            Description = "Enter your JWT token"
        });
        c.AddSecurityRequirement(new OpenApiSecurityRequirement
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

    // â”€â”€ Database (PostgreSQL) â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    var connectionString = builder.Configuration.BuildPostgresConnectionString();
    builder.Services.AddPostgresDataAccess<IdentityContext>(connectionString);

    // â”€â”€ Migration Strategy (environment-specific) â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    var environment = builder.Environment.EnvironmentName;
    new MigrationConfiguration(builder.Services)
        .WithEnvironment(environment)
        .AddContext<IdentityContext>()
        .Build();

    // â”€â”€ CQRS: handlers, validators, mappers â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    builder.Services.AddIdentityServices();

    // â”€â”€ Outbox Event Repository â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    builder.Services.AddScoped<IOutboxEventRepository, OutboxEventRepository>();

    // â”€â”€ Identity Services â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    builder.Services.AddScoped<IIdentityCacheService, IdentityCacheService>();

    // â”€â”€ Security â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    var encryptionKey = builder.Configuration["Security:EncryptionKey"]
        ?? Environment.GetEnvironmentVariable("ENCRYPTION_KEY")
        ?? throw new InvalidOperationException("ENCRYPTION_KEY secret is required.");

    builder.Services.AddSingleton<IEncryptionService>(new EncryptionService(encryptionKey));
    builder.Services.AddSingleton<IPasswordHasher, PasswordHasher>();

    var jwtSecret = builder.Configuration["Jwt:Secret"]
        ?? Environment.GetEnvironmentVariable("JWT_SECRET")
        ?? throw new InvalidOperationException("JWT_SECRET secret is required.");

    var jwtIssuer   = builder.Configuration["Jwt:Issuer"]   ?? "ehr-platform";
    var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "ehr-api";
    var jwtExpMin   = int.TryParse(builder.Configuration["Jwt:ExpirationMinutes"], out var m) ? m : 60;

    // Use building-blocks JWT provider
    var jwtSettings = new JwtSettings
    {
        SecretKey = jwtSecret,
        Issuer = jwtIssuer,
        Audience = jwtAudience,
        AccessTokenExpirationMinutes = jwtExpMin,
        RefreshTokenExpirationDays = 7,
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateIssuerSigningKey = true,
        ValidateLifetime = true,
        ClockSkewSeconds = 0
    };
    builder.Services.AddSingleton(jwtSettings);
    builder.Services.AddScoped<IJwtTokenProvider, JwtTokenProvider>();
    builder.Services.AddJwtAuthentication(jwtSecret, jwtIssuer, jwtAudience);

    // â”€â”€ Redis Caching (optional â€” degrades gracefully if unavailable) â”€â”€â”€â”€â”€â”€â”€â”€â”€
    var redisConnStr = builder.Configuration["Redis:ConnectionString"]
        ?? Environment.GetEnvironmentVariable("REDIS_CONNECTION_STRING");

    if (!string.IsNullOrEmpty(redisConnStr))
    {
        try
        {
            builder.Services.AddRedisCaching(redisConnStr);
            Log.Information("Redis caching enabled");
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Redis not available â€” caching disabled");
        }
    }
    else
    {
        Log.Warning("Redis:ConnectionString not configured â€” caching disabled");
    }

    // â”€â”€ Elasticsearch (optional â€” degrades gracefully if unavailable) â”€â”€â”€â”€â”€â”€â”€â”€â”€
    var esUrl = builder.Configuration["Elasticsearch:Url"]
        ?? Environment.GetEnvironmentVariable("ELASTICSEARCH_URL");

    if (!string.IsNullOrEmpty(esUrl))
    {
        try
        {
            builder.Services.AddElasticsearchSearch(esUrl);
            Log.Information("Elasticsearch enabled at {Url}", esUrl);
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Elasticsearch not available â€” search disabled");
        }
    }
    else
    {
        Log.Warning("Elasticsearch:Url not configured â€” search disabled");
    }

    // â”€â”€ CORS â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    builder.Services.AddCors(options =>
        options.AddPolicy("AllowAll", p =>
            p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

    // â”€â”€ Health checks â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    var healthBuilder = builder.Services.AddHealthChecks()
        .AddDbContextCheck<IdentityContext>("postgres-identity", tags: new[] { "db", "postgres" });

    // Redis health check â€” only when Redis is wired up
    if (!string.IsNullOrEmpty(redisConnStr))
        healthBuilder.AddCacheHealthCheck("redis-identity");

    // Elasticsearch health check â€” only when ES is wired up
    if (!string.IsNullOrEmpty(esUrl))
        healthBuilder.AddElasticsearchHealthCheck("elasticsearch-identity");

    // â”€â”€ Build â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    var app = builder.Build();

    // â”€â”€ Apply Migrations (environment-specific strategy) â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    try
    {
        await app.Services.RunMigrationsAsync<IdentityContext>("IdentityService");
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "Migration failed for IdentityService");
        if (app.Environment.IsProduction())
            throw;
    }

    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "EHR Identity Service v1");
        c.RoutePrefix = string.Empty; // serve Swagger UI at root "/"
    });

    app.UseSerilogRequestLogging();
    app.UseCors("AllowAll");
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();
    app.MapHealthChecks("/health");
    // app.MapPrometheusMetricsEndpoint();

    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Identity Service terminated unexpectedly");
    throw;
}
finally
{
    Log.CloseAndFlush();
}





