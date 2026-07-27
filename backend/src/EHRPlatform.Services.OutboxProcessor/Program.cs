using System.Reflection;
using Confluent.Kafka;
using EHRPlatform.Common.Data;
using EHRPlatform.Common.Events;
using EHRPlatform.Common.Extensions;
using EHRPlatform.Services.OutboxProcessor.Workers;
using Microsoft.EntityFrameworkCore;
using Serilog;

// ── Bootstrap logger ──────────────────────────────────────────────────────────
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .Enrich.FromLogContext()
    .CreateBootstrapLogger();

try
{
    var builder = Host.CreateDefaultBuilder(args)
        .UseSerilog((ctx, cfg) =>
            cfg.MinimumLevel.Information()
               .WriteTo.Console()
               .Enrich.FromLogContext());

    builder.ConfigureServices((ctx, services) =>
    {
        // ── OpenTelemetry ────────────────────────────────────────────────────
        services.AddOpenTelemetryObservability("outbox-processor");

        // ── Database: PostgreSQL (read all service databases) ──────────────────
        var postgresConn = ctx.Configuration.GetConnectionString("Postgres")
                        ?? Environment.GetEnvironmentVariable("POSTGRES_CONNECTION_STRING")
                        ?? throw new InvalidOperationException("PostgreSQL connection required");

        // Register DbContext factory for multi-service outbox reading
        services.AddDbContextFactory<MultiServiceOutboxDbContext>(o =>
            o.UseNpgsql(postgresConn,
                opts => opts.CommandTimeout(30)
                            .EnableRetryOnFailure(3, TimeSpan.FromSeconds(5), null)));

        // ── Kafka Producer ───────────────────────────────────────────────────
        var kafkaBootstrap = ctx.Configuration["Kafka:BootstrapServers"]
                          ?? Environment.GetEnvironmentVariable("KAFKA_BOOTSTRAP_SERVERS")
                          ?? "localhost:9092";

        var producerConfig = new ProducerConfig
        {
            BootstrapServers = kafkaBootstrap,
            ClientId = "ehr-outbox-processor",
            Acks = Acks.All,  // Wait for all in-sync replicas
            MessageTimeoutMs = 30000,
            CompressionType = CompressionType.Snappy
        };

        services.AddSingleton(new ProducerBuilder<string, string>(producerConfig)
            .SetErrorHandler((_, e) => Log.Error($"Kafka producer error: {e.Reason}"))
            .Build());

        // ── Outbox Processor Worker ──────────────────────────────────────────
        services.AddHostedService<OutboxProcessorWorker>();

        // ── Health Checks ────────────────────────────────────────────────────
        services.AddHealthChecks()
            .AddCheck("kafka", () => HealthCheckResult.Healthy("Kafka producer ready"), tags: ["readiness"]);
    });

    var host = builder.Build();

    // ── Run migrations on startup ─────────────────────────────────────────
    using (var scope = host.Services.CreateScope())
    {
        var dbContextFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<MultiServiceOutboxDbContext>>();
        using (var db = await dbContextFactory.CreateDbContextAsync())
        {
            Log.Information("Running database migrations...");
            await db.Database.MigrateAsync();
            Log.Information("Migrations completed");
        }
    }

    Log.Information("🚀 Outbox Processor starting...");
    await host.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Outbox Processor terminated unexpectedly");
    throw;
}
finally
{
    Log.CloseAndFlush();
}
