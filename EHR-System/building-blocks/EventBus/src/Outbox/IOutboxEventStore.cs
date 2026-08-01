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
