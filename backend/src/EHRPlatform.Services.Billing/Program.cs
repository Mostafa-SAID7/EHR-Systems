using EHRPlatform.Common.Extensions;
using EHRPlatform.Common.Security;
using EHRPlatform.Common.Data.Migrations;
using EHRPlatform.Services.Billing.Data;
using EHRPlatform.Services.Billing.Data.Repositories;
using EHRPlatform.Services.Billing.Data.Queries;
using EHRPlatform.Services.Billing.Application.Services;
using EHRPlatform.Services.Billing.Infrastructure.HealthChecks;
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
    builder.Services.AddOpenTelemetryObservability("billing-service");

    // ── Controllers & Swagger ─────────────────────────────────────────────────
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    // ── Database (PostgreSQL) ─────────────────────────────────────────────────
    var connectionString = builder.Configuration.BuildPostgresConnectionString();
    builder.Services.AddPostgresDataAccess<BillingContext>(connectionString);

    // ── Migration Strategy (environment-specific) ────────────────────────────────
    var environment = builder.Environment.EnvironmentName;
    new MigrationConfiguration(builder.Services)
        .WithEnvironment(environment)
        .AddContext<BillingContext>()
        .Build();

    // ── CQRS + Common ─────────────────────────────────────────────────────────
    builder.Services.AddCQRSFromCurrentAssembly();

    // ── Outbox Event Repository ────────────────────────────────────────────────
    builder.Services.AddScoped<IOutboxEventRepository, OutboxEventRepository>();

    // ── Billing Services ──────────────────────────────────────────────────────
    builder.Services.AddScoped<IBillingCacheService, BillingCacheService>();

    // ── Billing Dapper (invoice reporting & financial aggregations) ───────────
    builder.Services.AddScoped<IBillingDapperRepository, BillingDapperRepository>();

    // ── Redis Caching (optional) ──────────────────────────────────────────────
    var redisConnStr = builder.Configuration["Redis:ConnectionString"]
        ?? Environment.GetEnvironmentVariable("REDIS_CONNECTION_STRING");
    if (!string.IsNullOrEmpty(redisConnStr))
    {
        try { builder.Services.AddRedisCaching(redisConnStr); }
        catch (Exception ex) { Log.Warning(ex, "Redis not available for Billing Service"); }
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
            builder.Services.AddScoped<IBillingSearchService, BillingSearchService>();
            Log.Information("Elasticsearch initialized for Billing");
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
        .AddDbContextCheck<BillingContext>("postgres-billing", tags: ["db", "postgres"]);
    
    if (!string.IsNullOrEmpty(elasticsearchUrl))
    {
        healthChecks.AddCheck<ElasticsearchHealthCheck>("elasticsearch", tags: ["search"]);
    }
    
    if (!string.IsNullOrEmpty(redisConnStr))
    {
        healthChecks.AddRedis(redisConnStr, "redis-billing", tags: ["cache"]);
    }

    // ── Build ─────────────────────────────────────────────────────────────────
    var app = builder.Build();

    // ── Apply Migrations (environment-specific strategy) ────────────────────────
    try
    {
        await app.Services.RunMigrationsAsync<BillingContext>("BillingService");
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "Migration failed for BillingService");
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

    Log.Information("EHR Billing Service starting");
    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Billing Service terminated unexpectedly");
    throw;
}
finally
{
    Log.CloseAndFlush();
}


