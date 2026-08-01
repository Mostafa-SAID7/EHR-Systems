using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EHRPlatform.EventBus.Outbox;

/// <summary>
/// Interface for outbox event persistence.
/// Single responsibility: Store and retrieve outbox messages.
/// </summary>
public interface IOutboxEventStore
{
    /// <summary>
    /// Add event to outbox.
    /// </summary>
    Task<string> AddEventAsync(OutboxEventData eventData, CancellationToken cancellationToken = default);

    /// <summary>
    /// Add multiple events to outbox.
    /// </summary>
    Task AddEventsAsync(IEnumerable<OutboxEventData> events, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get unpublished events.
    /// </summary>
    Task<IReadOnlyList<OutboxEventData>> GetUnpublishedAsync(int maxCount = 100, CancellationToken cancellationToken = default);

    /// <summary>
    /// Mark event as published.
    /// </summary>
    Task MarkAsPublishedAsync(string eventId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Mark event as failed.
    /// </summary>
    Task MarkAsFailedAsync(string eventId, string errorMessage, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get event by ID.
    /// </summary>
    Task<OutboxEventData?> GetEventAsync(string eventId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete published events older than date.
    /// </summary>
    Task<int> DeletePublishedOlderThanAsync(DateTime cutoffDate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get outbox statistics.
    /// </summary>
    Task<OutboxStoreStats> GetStatsAsync(CancellationToken cancellationToken = default);
}

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

/// <summary>
/// Outbox store statistics.
/// Single responsibility: Outbox store statistics data.
/// </summary>
public class OutboxStoreStats
{
    /// <summary>
    /// Total pending events.
    /// </summary>
    public int PendingCount { get; set; }

    /// <summary>
    /// Total published events.
    /// </summary>
    public long PublishedCount { get; set; }

    /// <summary>
    /// Total failed events.
    /// </summary>
    public int FailedCount { get; set; }

    /// <summary>
    /// Oldest pending event creation time.
    /// </summary>
    public DateTime? OldestPendingAt { get; set; }
}
