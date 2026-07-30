#nullable enable

using System.Text.Json;
using EHRPlatform.Common.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EHRPlatform.Common.Messaging;

/// <summary>
/// Background service that periodically inspects outbox events that have exhausted
/// all publish retries and routes them to a Dead Letter Queue (DLQ) Kafka topic.
///
/// DLQ topic naming convention: dlq.{original-event-type}.{environment}
/// Example: dlq.patient-created-event.production
///
/// Operations on each DLQ event:
///   1. Publish to the DLQ Kafka topic with full metadata (error reason, attempt count).
///   2. Mark the outbox row as "dead-lettered" so the OutboxProcessor ignores it.
///
/// HIPAA: DLQ messages are stored on Kafka (encrypted at rest). Access is restricted
/// to the ops team. Payload may contain clinical data — treat as PHI.
/// </summary>
public sealed class DeadLetterQueueProcessor : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DeadLetterQueueProcessor> _logger;

    private const int PollIntervalSeconds = 30;
    private const string DlqTopicPrefix   = "dlq";

    public DeadLetterQueueProcessor(
        IServiceProvider serviceProvider,
        ILogger<DeadLetterQueueProcessor> logger)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _logger          = logger          ?? throw new ArgumentNullException(nameof(logger));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Dead Letter Queue processor starting (poll interval: {Interval}s)",
            PollIntervalSeconds);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessDeadLettersAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in DLQ processor loop");
            }

            await Task.Delay(TimeSpan.FromSeconds(PollIntervalSeconds), stoppingToken);
        }
    }

    private async Task ProcessDeadLettersAsync(CancellationToken cancellationToken)
    {
        using var scope     = _serviceProvider.CreateScope();
        var outboxRepo      = scope.ServiceProvider.GetRequiredService<IOutboxRepository>();
        var publisher       = scope.ServiceProvider.GetService<IEventPublisher>();

        var failed = (await outboxRepo.GetFailedAsync(cancellationToken)).ToList();
        if (failed.Count == 0) return;

        _logger.LogWarning("DLQ processor found {Count} dead-lettered event(s)", failed.Count);

        foreach (var outboxEvent in failed)
        {
            await RouteToDeadLetterAsync(outboxEvent, publisher, outboxRepo, cancellationToken);
        }
    }

    private async Task RouteToDeadLetterAsync(
        OutboxEvent outboxEvent,
        IEventPublisher? publisher,
        IOutboxRepository outboxRepo,
        CancellationToken cancellationToken)
    {
        _logger.LogError(
            "Dead-lettering event {EventId} ({EventType}) after {Attempts} attempt(s). LastError: {Error}",
            outboxEvent.Id,
            outboxEvent.EventType,
            outboxEvent.PublishAttempts,
            outboxEvent.ErrorMessage);

        // Guard: if no publisher is available we cannot confirm delivery to the DLQ.
        // Leave the event in its current failed state so operators can inspect it.
        // Never silently mark it as published when it has not actually been sent anywhere.
        if (publisher == null)
        {
            _logger.LogError(
                "No IEventPublisher is registered — cannot dead-letter event {EventId} ({EventType}). " +
                "Keeping it in the failed state for operator inspection. " +
                "Register IEventPublisher (e.g. via AddKafkaMessaging) to enable DLQ routing.",
                outboxEvent.Id, outboxEvent.EventType);
            return;
        }

        try
        {
            // 1. Publish to the DLQ Kafka topic first.
            var dlqEnvelope = new DeadLetterEnvelope(
                OriginalEventId:   outboxEvent.Id,
                OriginalEventType: outboxEvent.EventType,
                Payload:           outboxEvent.EventData,
                Attempts:          outboxEvent.PublishAttempts,
                LastError:         outboxEvent.ErrorMessage ?? "unknown",
                DeadLetteredAt:    DateTime.UtcNow);

            var dlqEvent = new DlqIntegrationEvent(dlqEnvelope);
            await publisher.PublishAsync(dlqEvent, cancellationToken);

            // 2. Only mark as published after the DLQ publish is confirmed.
            //    This ensures the outbox row is not silently discarded if step 1 failed.
            await outboxRepo.MarkAsPublishedAsync(outboxEvent.Id, cancellationToken);

            _logger.LogInformation(
                "Event {EventId} successfully routed to DLQ topic {DlqTopic}",
                outboxEvent.Id, dlqEvent.EventType);
        }
        catch (Exception ex)
        {
            // Publish or DB update failed — leave the event in the failed state.
            // The DLQ processor will retry on the next polling cycle.
            _logger.LogError(ex,
                "Failed to dead-letter event {EventId} — leaving in failed state, will retry on next cycle",
                outboxEvent.Id);
        }
    }
}

/// <summary>Envelope stored on the DLQ topic.</summary>
public sealed record DeadLetterEnvelope(
    Guid     OriginalEventId,
    string   OriginalEventType,
    string?  Payload,
    int      Attempts,
    string   LastError,
    DateTime DeadLetteredAt);

/// <summary>Integration event wrapper so DlqEnvelope can be published via IEventPublisher.</summary>
public sealed class DlqIntegrationEvent : IntegrationEvent
{
    public DeadLetterEnvelope Envelope { get; }
    private readonly string _eventType;

    public DlqIntegrationEvent(DeadLetterEnvelope envelope)
    {
        Envelope   = envelope ?? throw new ArgumentNullException(nameof(envelope));
        _eventType = $"dlq.{envelope.OriginalEventType}";
    }

    public override string EventType       => _eventType;
    public override string GetPartitionKey() => Envelope.OriginalEventId.ToString();
}
