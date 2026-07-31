#nullable enable

namespace EHRPlatform.Common.Domain.Events;

/// <summary>
/// Dead Letter Queue integration event wrapper.
/// Used when publishing failed messages from DLQ back to event system.
/// Wraps DeadLetterEnvelope so it can be published via IEventPublisher.
/// </summary>
public sealed class DlqIntegrationEvent : IntegrationEvent
{
    /// <summary>
    /// The dead letter envelope containing the failed event details.
    /// </summary>
    public DeadLetterEnvelope Envelope { get; }

    private readonly string _eventType;

    /// <summary>
    /// Create a DLQ integration event from a dead letter envelope.
    /// </summary>
    public DlqIntegrationEvent(DeadLetterEnvelope envelope)
    {
        Envelope   = envelope ?? throw new ArgumentNullException(nameof(envelope));
        _eventType = $"dlq.{envelope.OriginalEventType}";
    }

    /// <summary>
    /// DLQ events have a special topic prefixed with "dlq.".
    /// </summary>
    public override string EventType => _eventType;

    /// <summary>
    /// Partition key is the original event ID for correlation.
    /// </summary>
    public override string GetPartitionKey() => Envelope.OriginalEventId.ToString();
}

/// <summary>
/// Dead letter envelope for storing failed event details.
/// </summary>
public record DeadLetterEnvelope(
    /// <summary>Original event ID that failed.</summary>
    Guid OriginalEventId,

    /// <summary>Original event type name.</summary>
    string OriginalEventType,

    /// <summary>Number of failed delivery attempts.</summary>
    int Attempts,

    /// <summary>Last error message from failure.</summary>
    string LastError,

    /// <summary>When the event was sent to dead letter queue.</summary>
    DateTime DeadLetteredAt);
