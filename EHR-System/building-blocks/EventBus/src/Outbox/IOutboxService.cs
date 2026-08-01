using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EHRPlatform.EventBus.Outbox;

/// <summary>
/// Interface for outbox service management.
/// Single responsibility: Manage outbox operations.
/// </summary>
public interface IOutboxService
{
    /// <summary>
    /// Add domain event to outbox.
    /// </summary>
    Task AddEventAsync(object domainEvent, CancellationToken cancellationToken = default);

    /// <summary>
    /// Add multiple events to outbox.
    /// </summary>
    Task AddEventsAsync(IEnumerable<object> domainEvents, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get pending events.
    /// </summary>
    Task<IReadOnlyList<OutboxEventData>> GetPendingEventsAsync(int maxCount = 100, CancellationToken cancellationToken = default);

    /// <summary>
    /// Mark event as processed.
    /// </summary>
    Task MarkAsProcessedAsync(string eventId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete processed events older than date.
    /// </summary>
    Task<int> DeleteProcessedOlderThanAsync(DateTime cutoffDate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get service health status.
    /// </summary>
    Task<bool> IsHealthyAsync(CancellationToken cancellationToken = default);
}
