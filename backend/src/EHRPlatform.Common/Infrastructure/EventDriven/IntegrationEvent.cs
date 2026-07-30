namespace EHRPlatform.Common.Events;

/// <summary>
/// Base class for integration events published to external systems via Kafka.
/// Used for service-to-service communication and event-driven workflows.
/// </summary>
public abstract class IntegrationEvent
{
    /// <summary>Unique identifier for this integration event.</summary>
    public Guid EventId { get; set; } = Guid.NewGuid();

    /// <summary>When the event occurred.</summary>
    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;

    /// <summary>Correlation ID for tracing across services.</summary>
    public string? CorrelationId { get; set; }

    /// <summary>Causation ID – the command/event that triggered this one.</summary>
    public string? CausationId { get; set; }

    /// <summary>User ID who triggered this event.</summary>
    public Guid? UserId { get; set; }

    /// <summary>Tenant ID for multi-tenant systems.</summary>
    public Guid? TenantId { get; set; }

    /// <summary>Schema version for forward/backward compatibility.</summary>
    public int Version { get; set; } = 1;

    /// <summary>Source service that published this event.</summary>
    public string? SourceService { get; set; }

    /// <summary>Event type name used for Kafka topic routing.</summary>
    public virtual string EventType => GetType().Name;

    /// <summary>Kafka topic name derived from the event type.</summary>
    public virtual string Topic => EventType.ToKebabCase();

    /// <summary>Kafka partition key (defaults to TenantId or "default").</summary>
    public virtual string GetPartitionKey() =>
        TenantId?.ToString() ?? "default";
}

/// <summary>
/// Concrete integration event used internally by the outbox processor when
/// re-publishing stored events whose original type is not available at runtime.
/// </summary>
internal sealed class OutboxPublishableEvent : IntegrationEvent
{
    private readonly string _eventType;

    public OutboxPublishableEvent(Guid eventId, string eventType, DateTime occurredAt)
    {
        EventId = eventId;
        _eventType = eventType;
        OccurredAt = occurredAt;
    }

    public override string EventType => _eventType;
}

/// <summary>Extension methods for integration event string processing.</summary>
public static class IntegrationEventExtensions
{
    /// <summary>
    /// Converts PascalCase to kebab-case.
    /// e.g. "PatientCreatedEvent" → "patient-created-event"
    /// </summary>
    public static string ToKebabCase(this string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        var result = new System.Text.StringBuilder();
        for (int i = 0; i < input.Length; i++)
        {
            if (char.IsUpper(input[i]) && i > 0)
                result.Append('-');
            result.Append(char.ToLowerInvariant(input[i]));
        }

        return result.ToString();
    }
}
