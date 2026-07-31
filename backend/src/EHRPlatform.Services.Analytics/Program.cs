using EHRPlatform.Common.Application.Common.Extensions;
using EHRPlatform.Common.Infrastructure.Security;
using EHRPlatform.Common.Data.Migrations;
using EHRPlatform.Services.Analytics.Data;
using EHRPlatform.Services.Analytics.Data.Repositories;
using EHRPlatform.Services.Analytics.Application.Services;
using EHRPlatform.Services.Analytics.Infrastructure.HealthChecks;
using Elastic.Clients.Elasticsearch;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .Enrich.FromLogContext()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    // ── Logging ───────────────────────────────────────────────────────────────
    builder.Host.UseSerilog((ctx, config) =>
        config.ReadFrom.Configuration(ctx.Configuration));

    // ── OpenTelemetry Metrics ─────────────────────────────────────────────────
    builder.Services.AddOpenTelemetryObservability("analytics-service");

    // ── Controllers & Swagger ─────────────────────────────────────────────────
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    // ── Database (PostgreSQL) ─────────────────────────────────────────────────
    var connectionString = builder.Configuration.BuildPostgresConnectionString();
    builder.Services.AddPostgresDataAccess<AnalyticsContext>(connectionString);

    // ── Migration Strategy (environment-specific) ────────────────────────────────
    var environment = builder.Environment.EnvironmentName;
    new MigrationConfiguration(builder.Services)
        .WithEnvironment(environment)
        .AddContext<AnalyticsContext>()
        .Build();

    // ── CQRS + Common ─────────────────────────────────────────────────────────
    builder.Services.AddCQRSFromCurrentAssembly();

    // ── Outbox Event Repository ────────────────────────────────────────────────
    builder.Services.AddScoped<IOutboxEventRepository, OutboxEventRepository>();

    // ── Analytics Services ─────────────────────────────────────────────────────
    builder.Services.AddScoped<IAnalyticsCacheService, AnalyticsCacheService>();

    // ── Redis Caching (optional) ──────────────────────────────────────────────
    var redisConnStr = builder.Configuration["Redis:ConnectionString"]
        ?? Environment.GetEnvironmentVariable("REDIS_CONNECTION_STRING");
    if (!string.IsNullOrEmpty(redisConnStr))
    {
        try { builder.Services.AddRedisCaching(redisConnStr); }
        catch (Exception ex) { Log.Warning(ex, "Redis not available for Analytics Service"); }
    }

    // ── Elasticsearch Search (optional) ────────────────────────────────────────
    var elasticsearchUrl = builder.Configuration["Elasticsearch:Url"]
        ?? Environment.GetEnvironmentVariable("ELASTICSEARCH_URL");
    if (!string.IsNullOrEmpty(elasticsearchUrl))
    {
        try
        {
            var settings = new ElasticsearchClientSettings(new Uri(elasticsearchUrl));
            var client = new ElasticsearchClient(settings);
            builder.Services.AddSingleton(client);
            builder.Services.AddScoped<IAnalyticsSearchService, AnalyticsSearchService>();
            Log.Information("Elasticsearch initialized for Analytics");
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Elasticsearch not available - search disabled");
        }
    }

    // ── JWT Authentication ────────────────────────────────────────────────────
    var jwtSecret = builder.Configuration["Jwt:Secret"]
        ?? Environment.GetEnvironmentVariable("JWT_SECRET")
        ?? throw new InvalidOperationException("JWT_SECRET is required");
    builder.Services.AddJwtAuthentication(jwtSecret);

    // ── CORS ──────────────────────────────────────────────────────────────────
    builder.Services.AddCors(options =>
        options.AddPolicy("AllowAll", p => p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

    // ── Health Checks ─────────────────────────────────────────────────────────
    var healthChecks = builder.Services.AddHealthChecks()
        .AddDbContextCheck<AnalyticsContext>("postgres-analytics", tags: ["db", "postgres"]);
    
    if (!string.IsNullOrEmpty(elasticsearchUrl))
    {
        healthChecks.AddCheck<ElasticsearchHealthCheck>("elasticsearch", tags: ["search"]);
    }
    
    if (!string.IsNullOrEmpty(redisConnStr))
    {
        healthChecks.AddRedis(redisConnStr, "redis-analytics", tags: ["cache"]);
    }

    // ── Build ─────────────────────────────────────────────────────────────────
    var app = builder.Build();

    // ── Apply Migrations (environment-specific strategy) ────────────────────────
    try
    {
        await app.Services.RunMigrationsAsync<AnalyticsContext>("AnalyticsService");
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "Migration failed for AnalyticsService");
        if (app.Environment.IsProduction())
            throw;
    }

    app.UseSwagger();
    app.UseSwaggerUI();

    app.UseSerilogRequestLogging();
    app.UseCors("AllowAll");
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();
    app.MapHealthChecks("/health");
    // app.MapPrometheusMetricsEndpoint();

    Log.Information("EHR Analytics Service starting");
    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Analytics Service terminated unexpectedly");
    throw;
}
finally
{
    Log.CloseAndFlush();
}



