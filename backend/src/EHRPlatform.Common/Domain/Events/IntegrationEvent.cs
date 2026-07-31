#nullable enable

using EHRPlatform.Common.Shared.Utilities.Helpers;

namespace EHRPlatform.Common.Domain.Events;

/// <summary>
/// Base class for integration events published to external systems via Kafka.
/// Used for service-to-service communication and event-driven workflows.
/// Single responsibility: Define integration event contract only.
/// </summary>
public abstract class IntegrationEvent
{
    /// <summary>Unique identifier for this integration event.</summary>
    public Guid EventId { get; set; } = GuidHelper.NewGuid();

    /// <summary>When the event occurred.</summary>
    public DateTime OccurredAt { get; set; } = DateTimeHelper.UtcNow;

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
