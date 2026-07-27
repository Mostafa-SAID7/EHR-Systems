using EHRPlatform.Common.Extensions;
using EHRPlatform.Common.Health;
using EHRPlatform.Common.Security;
using EHRPlatform.Common.Data.Migrations;
using EHRPlatform.Services.Clinical.Application.Services;
using EHRPlatform.Services.Clinical.Data;
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
    builder.Services.AddOpenTelemetryObservability("clinical-service");

    // ── Controllers & Swagger ─────────────────────────────────────────────────
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    // ── Database (PostgreSQL) ─────────────────────────────────────────────────
    var connectionString = builder.Configuration.BuildPostgresConnectionString();
    builder.Services.AddPostgresDataAccess<ClinicalContext>(connectionString);

    // ── Migration Strategy (environment-specific) ────────────────────────────────
    var environment = builder.Environment.EnvironmentName;
    new MigrationConfiguration(builder.Services)
        .WithEnvironment(environment)
        .AddContext<ClinicalContext>()
        .Build();

    // ── CQRS + Common ─────────────────────────────────────────────────────────
    builder.Services.AddCQRSFromCurrentAssembly();

    // ── Cache Service (Wrapper for Redis) ──────────────────────────────────────
    builder.Services.AddScoped<ClinicalCacheService>();

    // ── Redis Caching (optional) ──────────────────────────────────────────────
    var redisConnStr = builder.Configuration["Redis:ConnectionString"]
        ?? Environment.GetEnvironmentVariable("REDIS_CONNECTION_STRING");
    if (!string.IsNullOrEmpty(redisConnStr))
    {
        try { builder.Services.AddRedisCaching(redisConnStr); }
        catch (Exception ex) { Log.Warning(ex, "Redis not available for Clinical Service"); }
    }

    // ── Elasticsearch (optional) ──────────────────────────────────────────────
    var esUrl = builder.Configuration["Elasticsearch:Uri"]
        ?? builder.Configuration["Elasticsearch:Url"]
        ?? Environment.GetEnvironmentVariable("ELASTICSEARCH_URL");
    if (!string.IsNullOrEmpty(esUrl))
    {
        try { builder.Services.AddElasticsearchSearch(esUrl); }
        catch (Exception ex) { Log.Warning(ex, "Elasticsearch not available for Clinical Service"); }
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
        .AddDbContextCheck<ClinicalContext>("postgres-clinical", tags: ["db", "postgres"]);
    if (!string.IsNullOrEmpty(redisConnStr))
        healthBuilder.AddCacheHealthCheck("redis-clinical");
    if (!string.IsNullOrEmpty(esUrl))
        healthBuilder.AddElasticsearchHealthCheck("elasticsearch-clinical");

    // ── Build ─────────────────────────────────────────────────────────────────
    var app = builder.Build();

    // ── Apply Migrations (environment-specific strategy) ────────────────────────
    try
    {
        await app.Services.RunMigrationsAsync<ClinicalContext>("ClinicalService");
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "Migration failed for ClinicalService");
        if (app.Environment.IsProduction())
            throw;
    }

    // ── Legacy: Schema verification ────────────────────────────────────────────
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<ClinicalContext>();
        await db.Database.EnsureCreatedAsync();
        Log.Information("Clinical database schema verified/created");
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

    Log.Information("EHR Clinical Service starting");
    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Clinical Service terminated unexpectedly");
    throw;
}
finally
{
    Log.CloseAndFlush();
}


