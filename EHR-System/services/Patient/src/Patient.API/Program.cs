using EHRPlatform.BuildingBlocks.Common.Data;
using EHRPlatform.BuildingBlocks.Common.Application.Common.Extensions;
using EHRPlatform.BuildingBlocks.Observability.HealthChecks;
using EHRPlatform.BuildingBlocks.Common.Search;
using EHRPlatform.BuildingBlocks.Security.Authentication;
using EHRPlatform.BuildingBlocks.Common.Data.Migrations;
using EHRPlatform.Services.Patient.Application;
using EHRPlatform.Services.Patient.Data;
using EHRPlatform.Services.Patient.Persistence.Repositories;
using EHRPlatform.Services.Patient.Application.Services;
using EHRPlatform.Services.Patient.Extensions;
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

    // ── Serilog ───────────────────────────────────────────────────────────────
    builder.Host.UseSerilog((ctx, cfg) =>
        cfg.MinimumLevel.Information()
           .WriteTo.Console()
           .Enrich.FromLogContext());

    // ── OpenTelemetry Metrics ─────────────────────────────────────────────────
    // Exposes /metrics endpoint for Prometheus scraping
    // Collects: HTTP metrics, runtime (GC, memory), process (CPU), ASP.NET Core
    builder.Services.AddOpenTelemetryObservability("patient-service");
    builder.Services.AddPatientMetrics();  // ← Add patient-specific metrics

    // ── Controllers & Swagger ─────────────────────────────────────────────────
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo { Title = "EHR Patient Service", Version = "v1" });
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

    // ── Database (PostgreSQL) ─────────────────────────────────────────────────
    var connectionString = builder.Configuration.BuildPostgresConnectionString();
    builder.Services.AddPostgresDataAccess<PatientContext>(connectionString);

    // ── Migration Strategy (environment-specific) ────────────────────────────────
    var environment = builder.Environment.EnvironmentName;
    new MigrationConfiguration(builder.Services)
        .WithEnvironment(environment)
        .AddContext<PatientContext>()
        .Build();

    // ── CQRS: handlers, validators, mappers ──────────────────────────────────
    builder.Services.AddApplicationServices();

    // ── Outbox Event Repository ────────────────────────────────────────────────
    builder.Services.AddScoped<IOutboxEventRepository, OutboxEventRepository>();

    // ── Patient Services ─────────────────────────────────────────────────────
    builder.Services.AddScoped<IPatientCacheService, PatientCacheService>();

    // ── Redis Caching (optional — degrades gracefully if unavailable) ─────────
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
            Log.Warning(ex, "Redis not available — caching disabled");
        }
    }
    else
    {
        Log.Warning("Redis:ConnectionString not configured — caching disabled");
    }

    // ── Elasticsearch (optional — degrades gracefully if unavailable) ─────────
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
            Log.Warning(ex, "Elasticsearch not available — search disabled");
        }
    }
    else
    {
        Log.Warning("Elasticsearch:Url not configured — search disabled");
    }

    // ── CORS ─────────────────────────────────────────────────────────────────
    builder.Services.AddCors(options =>
        options.AddPolicy("AllowAll", p =>
            p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

    // ── Health checks ─────────────────────────────────────────────────────────
    var healthBuilder = builder.Services.AddHealthChecks()
        .AddDbContextCheck<PatientContext>("postgres-patient", tags: new[] { "db", "postgres" });

    // Redis health check — only when Redis is wired up
    if (!string.IsNullOrEmpty(redisConnStr))
        healthBuilder.AddCacheHealthCheck("redis-patient");

    // Elasticsearch health check — only when ES is wired up
    if (!string.IsNullOrEmpty(esUrl))
        healthBuilder.AddElasticsearchHealthCheck("elasticsearch-patient");

    // ── Build ─────────────────────────────────────────────────────────────────
    var app = builder.Build();

    // ── Apply Migrations (environment-specific strategy) ────────────────────────
    try
    {
        await app.Services.RunMigrationsAsync<PatientContext>("PatientService");
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "Migration failed for PatientService");
        if (app.Environment.IsProduction())
            throw;
    }

    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "EHR Patient Service v1");
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
    Log.Fatal(ex, "Patient Service terminated unexpectedly");
    throw;
}
finally
{
    Log.CloseAndFlush();
}

