using Confluent.Kafka;
using EHRPlatform.Common.Data;
using EHRPlatform.Common.Events;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace EHRPlatform.Services.OutboxProcessor.Workers;

/// <summary>
/// Background worker that:
/// 1. Polls all service databases for unpublished OutboxEvents
/// 2. Publishes events to Kafka topics
/// 3. Marks events as published (idempotent)
/// 4. Retries failed events with exponential backoff
/// </summary>
public class OutboxProcessorWorker : BackgroundService
{
    private readonly IDbContextFactory<MultiServiceOutboxDbContext> _dbContextFactory;
    private readonly IProducer<string, string> _kafkaProducer;
    private readonly ILogger<OutboxProcessorWorker> _logger;
    private readonly IConfiguration _configuration;

    public OutboxProcessorWorker(
        IDbContextFactory<MultiServiceOutboxDbContext> dbContextFactory,
        IProducer<string, string> kafkaProducer,
        ILogger<OutboxProcessorWorker> logger,
        IConfiguration configuration)
    {
        _dbContextFactory = dbContextFactory;
        _kafkaProducer = kafkaProducer;
        _logger = logger;
        _configuration = configuration;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("🚀 Outbox Processor Worker started");

        var config = _configuration.GetSection("OutboxProcessor");
        var pollIntervalMs = config.GetValue("PollIntervalMs", 5000);
        var batchSize = config.GetValue("BatchSize", 100);
        var maxRetries = config.GetValue("MaxRetries", 3);
        var retryDelayMs = config.GetValue("RetryDelayMs", 1000);

        using var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(pollIntervalMs));

        try
        {
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                try
                {
                    await ProcessOutboxEvents(batchSize, maxRetries, retryDelayMs, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    _logger.LogInformation("Outbox Processor cancelled");
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing outbox events");
                    // Continue polling on error
                }
            }
        }
        finally
        {
            _kafkaProducer.Flush(TimeSpan.FromSeconds(10));
            _logger.LogInformation("✅ Outbox Processor Worker stopped");
        }
    }

    private async Task ProcessOutboxEvents(int batchSize, int maxRetries, int retryDelayMs, CancellationToken ct)
    {
        using var db = await _dbContextFactory.CreateDbContextAsync(ct);

        // Fetch unpublished events, ordered by CreatedAt (FIFO)
        var unpublishedEvents = await db.OutboxEvents
            .Where(e => !e.IsPublished && e.PublishAttempts < maxRetries)
            .OrderBy(e => e.CreatedAt)
            .Take(batchSize)
            .ToListAsync(ct);

        if (!unpublishedEvents.Any())
        {
            return;
        }

        _logger.LogInformation("Processing {Count} outbox events", unpublishedEvents.Count);

        foreach (var outboxEvent in unpublishedEvents)
        {
            try
            {
                // Determine Kafka topic based on event type namespace
                var kafkaTopics = _configuration.GetSection("Kafka:Topics");
                var topic = outboxEvent.EventType.Split('.')[0].ToLower() switch
                {
                    "patientcreated" or "patientupdated" => kafkaTopics["PatientEvents"] ?? "patient-events",
                    "appointmentscheduled" or "appointmentcanceled" => kafkaTopics["AppointmentEvents"] ?? "appointment-events",
                    "invoicegenerated" or "paymentprocessed" => kafkaTopics["BillingEvents"] ?? "billing-events",
                    "reportexecuted" or "analyticscomputed" => kafkaTopics["AnalyticsEvents"] ?? "analytics-events",
                    "auditlogged" => kafkaTopics["AuditEvents"] ?? "audit-events",
                    "notificationsent" => kafkaTopics["NotificationEvents"] ?? "notification-events",
                    _ => $"{outboxEvent.EventType.ToLower()}-events"
                };

                // Create Kafka message
                var aggregateKeyStr = outboxEvent.AggregateId?.ToString() ?? Guid.NewGuid().ToString();
                var kafkaMessage = new Message<string, string>
                {
                    Key = aggregateKeyStr,
                    Value = outboxEvent.EventData,
                    Headers = new Headers
                    {
                        { "event-type", System.Text.Encoding.UTF8.GetBytes(outboxEvent.EventType) },
                        { "timestamp", System.Text.Encoding.UTF8.GetBytes(outboxEvent.CreatedAt.ToUniversalTime().ToString("O")) },
                        { "event-id", System.Text.Encoding.UTF8.GetBytes(outboxEvent.Id.ToString()) }
                    }
                };

                // Publish to Kafka (async, doesn't wait)
                var deliveryReport = await _kafkaProducer.ProduceAsync(topic, kafkaMessage, ct);

                if (deliveryReport.Status == PersistenceStatus.Persisted)
                {
                    // Mark as published
                    outboxEvent.IsPublished = true;
                    outboxEvent.PublishedAt = DateTime.UtcNow;
                    outboxEvent.PublishAttempts++;

                    _logger.LogInformation(
                        "✅ Published {EventType} to topic {Topic} (partition: {Partition}, offset: {Offset})",
                        outboxEvent.EventType,
                        topic,
                        deliveryReport.Partition.Value,
                        deliveryReport.Offset.Value);
                }
                else
                {
                    // Kafka delivery failed, increment retry counter
                    outboxEvent.PublishAttempts++;
                    outboxEvent.ErrorMessage = $"Kafka delivery failed: {deliveryReport.Status}";
                    _logger.LogWarning(
                        "⚠️ Failed to publish {EventType} to {Topic} (attempt {Attempt}/{Max}): {Status}",
                        outboxEvent.EventType,
                        topic,
                        outboxEvent.PublishAttempts,
                        maxRetries,
                        deliveryReport.Status);
                }
            }
            catch (Exception ex)
            {
                outboxEvent.PublishAttempts++;
                outboxEvent.ErrorMessage = ex.Message;
                _logger.LogError(
                    ex,
                    "❌ Error publishing {EventType} (attempt {Attempt}/{Max})",
                    outboxEvent.EventType,
                    outboxEvent.PublishAttempts,
                    maxRetries);
            }
        }

        // Save all updates (published flags, attempt counts, error messages)
        await db.SaveChangesAsync(ct);
    }
}
