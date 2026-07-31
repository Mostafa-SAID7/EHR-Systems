#nullable enable

using EHRPlatform.Common.Shared.Utilities.Helpers;

namespace EHRPlatform.Common.Domain.Events;

/// <summary>
/// Outbox event for guaranteed message delivery pattern (Outbox Pattern).
/// Events are stored in database before publishing to ensure no events are lost.
/// BackgroundService periodically publishes to Kafka/RabbitMQ.
/// Single responsibility: Store event data for reliable delivery only.
/// </summary>
public class OutboxEvent
{
    /// <summary>
    /// Unique ID for this outbox event.
    /// </summary>
    public Guid Id { get; set; } = GuidHelper.NewGuid();

    /// <summary>
    /// Type name of the domain event.
    /// Used to deserialize the event and route to handlers.
    /// </summary>
    public string EventType { get; set; } = string.Empty;

    /// <summary>
    /// Serialized event data (JSON format).
    /// </summary>
    public string EventData { get; set; } = string.Empty;

    /// <summary>
    /// When this event was created in UTC.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTimeHelper.UtcNow;

    /// <summary>
    /// Whether this event has been published to message broker.
    /// </summary>
    public bool IsPublished { get; set; } = false;

    /// <summary>
    /// When this event was successfully published.
    /// Null until successfully published.
    /// </summary>
    public DateTime? PublishedAt { get; set; }

    /// <summary>
    /// Number of times publishing has been attempted.
    /// </summary>
    public int PublishAttempts { get; set; } = 0;

    /// <summary>
    /// Maximum number of times to attempt publishing before giving up.
    /// </summary>
    public int MaxPublishAttempts { get; set; } = 3;

    /// <summary>
    /// Error message from last failed attempt (if any).
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Whether this event should be retried (not yet published and below max attempts).
    /// </summary>
    public bool ShouldRetry => !IsPublished && PublishAttempts < MaxPublishAttempts;

    /// <summary>
    /// Aggregate root ID that raised this event (e.g. Patient ID).
    /// Used for partitioning and correlation.
    /// </summary>
    public Guid? AggregateId { get; set; }

    /// <summary>
    /// Target transport for this event: "kafka" (default) or "rabbitmq".
    /// Outbox processor routes accordingly.
    /// </summary>
    public string Transport { get; set; } = "kafka";

    /// <summary>
    /// Optional routing key / queue name used when Transport = "rabbitmq".
    /// </summary>
    public string? RoutingKey { get; set; }
}
