using EHRPlatform.Common.Application.Common.Extensions;
using EHRPlatform.Common.Infrastructure.Health;
using EHRPlatform.Common.Infrastructure.Security;
using EHRPlatform.Common.Data.Migrations;
using EHRPlatform.Services.Audit.Data;
using EHRPlatform.Services.Audit.Data.Repositories;
using EHRPlatform.Services.Audit.Data.Queries;
using EHRPlatform.Services.Audit.Application.Services;
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
    builder.Host.UseSerilog((ctx, cfg) =>
        cfg.MinimumLevel.Information()
           .WriteTo.Console()
           .Enrich.FromLogContext());

    // ── OpenTelemetry Metrics ─────────────────────────────────────────────────
    builder.Services.AddOpenTelemetryObservability("audit-service");

    // ── Controllers & Swagger ─────────────────────────────────────────────────
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    // ── Database (PostgreSQL) ─────────────────────────────────────────────────
    var connectionString = builder.Configuration.BuildPostgresConnectionString();
    builder.Services.AddPostgresDataAccess<AuditContext>(connectionString);

    // ── Migration Strategy (environment-specific) ────────────────────────────────
    var environment = builder.Environment.EnvironmentName;
    new MigrationConfiguration(builder.Services)
        .WithEnvironment(environment)
        .AddContext<AuditContext>()
        .Build();

    // ── CQRS ─────────────────────────────────────────────────────────────────
    builder.Services.AddCQRSFromCurrentAssembly();

    // ── Outbox Event Repository ────────────────────────────────────────────────
    builder.Services.AddScoped<IOutboxEventRepository, OutboxEventRepository>();

    // ── Audit Services ────────────────────────────────────────────────────────
    builder.Services.AddScoped<IAuditCacheService, AuditCacheService>();

    // ── Audit Dapper (bulk HIPAA compliance read queries) ─────────────────────
    builder.Services.AddScoped<IAuditDapperRepository, AuditDapperRepository>();

    // ── Redis Caching (optional) ──────────────────────────────────────────────
    var redisConnStr = builder.Configuration["ConnectionStrings:Redis"]
        ?? builder.Configuration["Redis:ConnectionString"]
        ?? Environment.GetEnvironmentVariable("REDIS_CONNECTION_STRING");
    if (!string.IsNullOrEmpty(redisConnStr))
    {
        try { builder.Services.AddRedisCaching(redisConnStr); }
        catch (Exception ex) { Log.Warning(ex, "Redis not available for Audit Service"); }
    }

    // ── Elasticsearch (optional — used for audit log search) ─────────────────
    var esNodes = builder.Configuration.GetSection("Elasticsearch:Nodes").Get<string[]>();
    var esUrl = (esNodes?.FirstOrDefault())
        ?? builder.Configuration["Elasticsearch:Url"]
        ?? Environment.GetEnvironmentVariable("ELASTICSEARCH_URL");
    if (!string.IsNullOrEmpty(esUrl))
    {
        try { builder.Services.AddElasticsearchSearch(esUrl); }
        catch (Exception ex) { Log.Warning(ex, "Elasticsearch not available for Audit Service"); }
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
    var healthBuilder = builder.Services.AddHealthChecks()
        .AddDbContextCheck<AuditContext>("postgres-audit", tags: ["db", "postgres"]);
    if (!string.IsNullOrEmpty(redisConnStr))
        healthBuilder.AddCacheHealthCheck("redis-audit");
    if (!string.IsNullOrEmpty(esUrl))
        healthBuilder.AddElasticsearchHealthCheck("elasticsearch-audit");

    // ── Build ─────────────────────────────────────────────────────────────────
    var app = builder.Build();

    // ── Apply Migrations (environment-specific strategy) ────────────────────────
    try
    {
        await app.Services.RunMigrationsAsync<AuditContext>("AuditService");
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "Migration failed for AuditService");
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

    Log.Information("EHR Audit Service starting");
    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Audit Service terminated unexpectedly");
    throw;
}
finally
{
    Log.CloseAndFlush();
}


