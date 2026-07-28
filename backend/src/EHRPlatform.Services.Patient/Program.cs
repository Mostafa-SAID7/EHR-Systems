using EHRPlatform.Common.Extensions;
using EHRPlatform.Common.Health;
using EHRPlatform.Common.Messaging;
using EHRPlatform.Common.Security;
using EHRPlatform.Common.Data.Migrations;
using EHRPlatform.Services.Patient.Data;
using EHRPlatform.Services.Patient.Messaging.Consumers;
using EHRPlatform.Services.Patient.Sagas;
using EHRPlatform.Services.Patient.Application.Services;
using EHRPlatform.Services.Patient.Infrastructure.HealthChecks;
using Elastic.Clients.Elasticsearch;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// ── Logging ──────────────────────────────────────────────────────────────────
builder.Host.UseSerilog((ctx, config) =>
    config.ReadFrom.Configuration(ctx.Configuration));

// ── Controllers & Swagger ─────────────────────────────────────────────────────
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ── Database (PostgreSQL via Replit env vars or explicit connection string) ────
var connectionString = builder.Configuration.BuildPostgresConnectionString();
builder.Services.AddPostgresDataAccess<PatientContext>(connectionString);

// ── Migration Strategy (environment-specific) ────────────────────────────────
var environment = builder.Environment.EnvironmentName;
new MigrationConfiguration(builder.Services)
    .WithEnvironment(environment)
    .AddContext<PatientContext>()
    .Build();

// ── Outbox repository (writes domain events atomically with patient data) ──────
builder.Services.AddScoped<IOutboxRepository>(sp =>
    new OutboxRepository(sp.GetRequiredService<PatientContext>()));

// ── CQRS (MediatR + FluentValidation + pipeline behaviors) ───────────────────
builder.Services.AddCQRSFromCurrentAssembly();

// ── Patient Services ──────────────────────────────────────────────────────────
builder.Services.AddScoped<IPatientCacheService, PatientCacheService>();
builder.Services.AddScoped<IPatientSearchService, PatientSearchService>();

// ── Redis Caching (optional — degrades gracefully if unavailable) ─────────────
var redisConnStr = builder.Configuration["Redis:ConnectionString"]
    ?? builder.Configuration["EHRCommon:RedisConnectionString"]
    ?? Environment.GetEnvironmentVariable("REDIS_CONNECTION_STRING");

if (!string.IsNullOrEmpty(redisConnStr))
{
    try
    {
        builder.Services.AddRedisCaching(redisConnStr);
        Log.Information("Redis caching enabled for Patient Service");
    }
    catch (Exception ex)
    {
        Log.Warning(ex, "Redis not available for Patient Service — caching disabled");
    }
}
else
{
    Log.Warning("Redis connection string not configured — caching disabled");
}

// ── Elasticsearch (optional — degrades gracefully if unavailable) ─────────────
var esUrl = builder.Configuration["Elasticsearch:Url"]
    ?? Environment.GetEnvironmentVariable("ELASTICSEARCH_URL");

if (!string.IsNullOrEmpty(esUrl))
{
    try
    {
        builder.Services.AddElasticsearchSearch(esUrl);
        var settings = new ElasticsearchClientSettings(new Uri(esUrl));
        var client = new ElasticsearchClient(settings);
        builder.Services.AddSingleton(client);
        Log.Information("Elasticsearch enabled for Patient Service at {Url}", esUrl);
    }
    catch (Exception ex)
    {
        Log.Warning(ex, "Elasticsearch not available — search disabled for Patient Service");
    }
}
else
{
    Log.Warning("Elasticsearch:Url not configured — patient search disabled");
}

// ── MongoDB (optional — used for clinical documents, audit logs, device data) ──
var mongoConnStr  = builder.Configuration["MongoDB:ConnectionString"]
    ?? Environment.GetEnvironmentVariable("MONGODB_CONNECTION_STRING");
var mongoDbName   = builder.Configuration["MongoDB:DatabaseName"] ?? "ehr_patient";

if (!string.IsNullOrEmpty(mongoConnStr))
{
    try
    {
        builder.Services.AddMongoDataAccess(mongoConnStr, mongoDbName);
        Log.Information("MongoDB enabled for Patient Service, database: {Db}", mongoDbName);
    }
    catch (Exception ex)
    {
        Log.Warning(ex, "MongoDB not available — document store disabled for Patient Service");
    }
}
else
{
    Log.Warning("MongoDB:ConnectionString not configured — document store disabled");
}

// ── Kafka raw publisher + resilience decorator (outbox uses this) ─────────────
var kafkaServers = builder.Configuration["Kafka:BootstrapServers"]
    ?? Environment.GetEnvironmentVariable("KAFKA_BOOTSTRAP_SERVERS");

if (!string.IsNullOrWhiteSpace(kafkaServers))
{
    builder.Services.AddKafkaMessaging(kafkaServers, builder.Environment.EnvironmentName);
    builder.Services.AddResilientEventPublisher();
    Log.Information("Kafka messaging enabled for Patient Service");
}
else
{
    Log.Warning("Kafka:BootstrapServers not configured — event publishing disabled");
}

// ── MassTransit: Kafka (domain events) + RabbitMQ (background jobs + saga) ───
var rabbitHost = builder.Configuration["RabbitMQ:Host"]
    ?? Environment.GetEnvironmentVariable("RABBITMQ_HOST");
var messagingTransportsConfigured =
    !string.IsNullOrWhiteSpace(kafkaServers) &&
    !string.IsNullOrWhiteSpace(rabbitHost);

if (messagingTransportsConfigured)
{
    builder.Services.AddMassTransitHybrid(
        builder.Configuration,
        configureRabbitMqConsumers: x =>
        {
            x.AddConsumer<WelcomeNotificationConsumer>();
            x.AddConsumer<PatientIndexConsumer>();
            x.AddSagaStateMachine<PatientRegistrationSaga, PatientRegistrationSagaState>()
                .EntityFrameworkRepository(r =>
                {
                    r.ExistingDbContext<PatientContext>();
                    r.UsePostgres();
                });
        },
        configureKafkaRider: rider =>
        {
            rider.AddConsumer<PatientCreatedKafkaConsumer>();
            rider.AddProducer<EHRPlatform.Services.Patient.Domain.Events.PatientCreatedEvent>(
                "patient-created-event");
        });
}
else
{
    // Keep the service usable locally when external brokers are not provisioned.
    builder.Services.AddMassTransit(x =>
    {
        x.AddConsumer<WelcomeNotificationConsumer>();
        x.AddConsumer<PatientIndexConsumer>();
        x.AddSagaStateMachine<PatientRegistrationSaga, PatientRegistrationSagaState>()
            .EntityFrameworkRepository(r =>
            {
                r.ExistingDbContext<PatientContext>();
                r.UsePostgres();
            });
        x.UsingInMemory((ctx, cfg) => cfg.ConfigureEndpoints(ctx));
    });
    builder.Services.AddScoped<IMessageBus, EHRMessageBus>();
    Log.Warning("Kafka/RabbitMQ not configured — Patient Service messaging is in-memory only");
}

// ── OpenTelemetry ─────────────────────────────────────────────────────────────
builder.Services.AddEHRTelemetry(builder.Configuration, "patient-service");
builder.Services.AddOpenTelemetryObservability("patient-service");

// ── JWT Authentication ────────────────────────────────────────────────────────
var jwtSecret = builder.Configuration["Jwt:Secret"]
    ?? Environment.GetEnvironmentVariable("JWT_SECRET")
    ?? throw new InvalidOperationException("JWT_SECRET not configured");
builder.Services.AddJwtAuthentication(jwtSecret);

// ── CORS ──────────────────────────────────────────────────────────────────────
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

// ── Health Checks ─────────────────────────────────────────────────────────────
var healthBuilder = builder.Services.AddHealthChecks()
    .AddDbContextCheck<PatientContext>("postgres-patient", tags: new[] { "db", "postgres" });

if (!string.IsNullOrEmpty(redisConnStr))
    healthBuilder.AddCacheHealthCheck("redis-patient");

if (!string.IsNullOrEmpty(esUrl))
    healthBuilder.AddElasticsearchHealthCheck("elasticsearch-patient");

if (!string.IsNullOrEmpty(mongoConnStr))
    healthBuilder.AddMongoHealthCheck("mongodb-patient");

// ── Build ─────────────────────────────────────────────────────────────────────
var app = builder.Build();

// ── Apply Migrations (environment-specific strategy) ────────────────────────────
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


// ── Pipeline ──────────────────────────────────────────────────────────────────
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseSerilogRequestLogging();
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");
// app.MapPrometheusMetricsEndpoint();

await app.RunAsync();


