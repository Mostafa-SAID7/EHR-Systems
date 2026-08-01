using System;

namespace EHRPlatform.EventBus.Outbox;

/// <summary>
/// Outbox message data model for transactional event publishing.
/// Single responsibility: Outbox message data structure.
/// </summary>
public class OutboxMessage
{
    /// <summary>
    /// Unique identifier for this outbox entry.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Aggregate ID (root entity this change belongs to).
    /// </summary>
    public Guid AggregateId { get; set; }

    /// <summary>
    /// Aggregate type (e.g., "Patient", "Appointment").
    /// </summary>
    public string AggregateType { get; set; } = null!;

    /// <summary>
    /// Event type name (e.g., "PatientCreatedIntegrationEvent").
    /// </summary>
    public string EventType { get; set; } = null!;

    /// <summary>
    /// Event data serialized as JSON.
    /// </summary>
    public string EventData { get; set; } = null!;

    /// <summary>
    /// When the outbox entry was created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// When the event was published to message broker.
    /// Null if not yet published.
    /// </summary>
    public DateTime? PublishedAt { get; set; }

    /// <summary>
    /// Number of publish attempts.
    /// </summary>
    public int PublishAttempts { get; set; }

    /// <summary>
    /// Maximum number of publish attempts before giving up.
    /// </summary>
    public int MaxPublishAttempts { get; set; } = 3;

    /// <summary>
    /// Error message if publish failed.
    /// </summary>
    public string? Error { get; set; }

    /// <summary>
    /// Correlation ID for tracing.
    /// </summary>
    public string? CorrelationId { get; set; }
}
