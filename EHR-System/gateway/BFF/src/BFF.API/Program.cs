using EHRPlatform.Services.BFF.Application;
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
    var builder = WebApplicationBuilder.CreateBuilder(args);

    // ── Serilog ───────────────────────────────────────────────────────────────
    builder.Host.UseSerilog((ctx, cfg) =>
        cfg.MinimumLevel.Information()
           .WriteTo.Console()
           .Enrich.FromLogContext());

    // ── OpenTelemetry Metrics ─────────────────────────────────────────────────
    builder.Services.AddOpenTelemetryObservability("bff");
    builder.Services.AddBFFApplicationServices();

    // ── HTTP Client Factory for resilience ────────────────────────────────────
    builder.Services
        .AddHttpClient()
        .AddStandardResilienceHandler();

    // ── Redis caching (for aggregation results) ───────────────────────────────
    var redisConn = builder.Configuration.GetConnectionString("Redis")
                 ?? Environment.GetEnvironmentVariable("REDIS_CONNECTION_STRING");
    if (!string.IsNullOrEmpty(redisConn))
    {
        builder.Services.AddStackExchangeRedisCache(o => o.Configuration = redisConn);
        Log.Information("Redis caching enabled for BFF");
    }

    // ── Swagger / OpenAPI ─────────────────────────────────────────────────────
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new OpenApiInfo
        {
            Title       = "BFF (Backend-For-Frontend)",
            Version     = "v1",
            Description = "Client-specific data aggregation and optimization layer"
        });
    });

    // ── CORS ──────────────────────────────────────────────────────────────────
    builder.Services.AddCors(options =>
        options.AddPolicy("AllowFrontend", p =>
            p.WithOrigins(builder.Configuration["Cors:AllowedOrigins"]?.Split(';') ?? new[] { "*" })
             .AllowAnyMethod()
             .AllowAnyHeader()));

    // ── Health checks ─────────────────────────────────────────────────────────
    builder.Services.AddHealthChecks();

    builder.Services.AddControllers();

    var app = builder.Build();

    // ── Middleware pipeline ───────────────────────────────────────────────────
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();
    app.UseCors("AllowFrontend");
    app.UseAuthorization();

    app.MapHealthChecks("/health");
    app.MapControllers();

    Log.Information("BFF starting on port 5001");
    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "BFF terminated unexpectedly");
    throw;
}
finally
{
    Log.CloseAndFlush();
}
