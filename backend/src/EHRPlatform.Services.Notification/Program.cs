using EHRPlatform.Common.Extensions;
using EHRPlatform.Services.Notification.Consumers;
using EHRPlatform.Services.Notification.Hubs;
using EHRPlatform.Services.Notification.Data;
using EHRPlatform.Services.Notification.Data.Repositories;
using EHRPlatform.Services.Notification.Application.Services;
using MassTransit;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// ── Logging ───────────────────────────────────────────────────────────────────
builder.Host.UseSerilog((ctx, config) =>
    config.ReadFrom.Configuration(ctx.Configuration));

// ── OpenTelemetry Metrics ─────────────────────────────────────────────────────
builder.Services.AddOpenTelemetryObservability("notification-service");

// ── Database (PostgreSQL) ─────────────────────────────────────────────────────
var connectionString = builder.Configuration.BuildPostgresConnectionString();
builder.Services.AddPostgresDataAccess<NotificationContext>(connectionString);

// ── Outbox Event Repository ────────────────────────────────────────────────────
builder.Services.AddScoped<IOutboxEventRepository, OutboxEventRepository>();

// ── Notification Services ────────────────────────────────────────────────────
builder.Services.AddScoped<INotificationCacheService, NotificationCacheService>();

// ── Redis Caching (optional) ──────────────────────────────────────────────────
var redisConnStr = builder.Configuration["Redis:ConnectionString"]
    ?? Environment.GetEnvironmentVariable("REDIS_CONNECTION_STRING");
if (!string.IsNullOrEmpty(redisConnStr))
{
    try { builder.Services.AddRedisCaching(redisConnStr); }
    catch (Exception ex) { Log.Warning(ex, "Redis not available for Notification Service"); }
}

// ── SignalR ───────────────────────────────────────────────────────────────────
builder.Services.AddSignalR(opts =>
{
    opts.EnableDetailedErrors = builder.Environment.IsDevelopment();
    opts.MaximumReceiveMessageSize = 32 * 1024; // 32KB
});

// ── MassTransit: RabbitMQ (background jobs) + Kafka rider (domain events) ────
builder.Services.AddMassTransitHybrid(
    builder.Configuration,
    configureRabbitMqConsumers: x =>
    {
        x.AddConsumer<SendWelcomeNotificationConsumer>();
    },
    configureKafkaRider: rider =>
    {
        // Bridge: Kafka domain events → SignalR push
        rider.AddConsumer<LabResultConsumer>();
        // Add more Kafka consumers here as clinical events are added:
        //   rider.AddConsumer<VitalAlertConsumer>();
        //   rider.AddConsumer<AppointmentReminderConsumer>();
    });

// ── OpenTelemetry ─────────────────────────────────────────────────────────────
builder.Services.AddEHRTelemetry(builder.Configuration, "notification-service");

// ── Health Checks ─────────────────────────────────────────────────────────────
builder.Services.AddHealthChecks();

// ── CORS (must allow SignalR WebSocket connections) ───────────────────────────
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
    ?? ["http://localhost:4200", "http://localhost:5000"];

builder.Services.AddCors(options =>
{
    options.AddPolicy("SignalRCors", policy =>
        policy.WithOrigins(allowedOrigins)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials()); // Required for SignalR WebSocket
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ── Build ─────────────────────────────────────────────────────────────────────
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("SignalRCors");
app.UseSerilogRequestLogging();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<EHRNotificationHub>("/hubs/notifications");
app.MapHealthChecks("/health");
// app.MapPrometheusMetricsEndpoint();

await app.RunAsync();

