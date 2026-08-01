using System;

namespace EHRPlatform.EventBus.Outbox;

/// <summary>
/// Outbox event data structure.
/// Single responsibility: Outbox event representation.
/// </summary>
public class OutboxEventData
{
    /// <summary>
    /// Event ID (unique).
    /// </summary>
    public string Id { get; set; } = null!;

    /// <summary>
    /// Event type name.
    /// </summary>
    public string EventType { get; set; } = null!;

    /// <summary>
    /// Event payload (JSON).
    /// </summary>
    public string Payload { get; set; } = null!;

    /// <summary>
    /// Event creation time.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Publication status.
    /// </summary>
    public OutboxEventStatus Status { get; set; } = OutboxEventStatus.Pending;

    /// <summary>
    /// Publication time (if published).
    /// </summary>
    public DateTime? PublishedAt { get; set; }

    /// <summary>
    /// Retry count.
    /// </summary>
    public int RetryCount { get; set; }

    /// <summary>
    /// Last error message (if failed).
    /// </summary>
    public string? Error { get; set; }

    /// <summary>
    /// Correlation ID for tracing.
    /// </summary>
    public string? CorrelationId { get; set; }
}
