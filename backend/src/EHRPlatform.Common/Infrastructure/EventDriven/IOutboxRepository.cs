#nullable enable

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EHRPlatform.Common.Events;

namespace EHRPlatform.Common.Messaging;

/// <summary>
/// Outbox repository for managing unpublished integration events.
/// Ensures guaranteed event delivery via outbox pattern.
/// 
/// Pattern:
/// 1. Entity changes + OutboxEvent created in same transaction
/// 2. Transaction commits (all-or-nothing)
/// 3. OutboxProcessor polls for unpublished events
/// 4. Publishes to Kafka with retry logic
/// 5. Marks as published on success
/// 6. Failed events go to dead letter queue
/// 
/// HIPAA: Events are stored in database before publishing
/// to guarantee no events are lost during service restarts.
/// </summary>
public interface IOutboxRepository
{
    /// <summary>
    /// Get all unpublished events.
    /// Called by OutboxProcessor to find events to publish.
    /// </summary>
    Task<IEnumerable<OutboxEvent>> GetUnpublishedAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get failed events (exceeded retry limit).
    /// These should be moved to dead letter queue for manual inspection.
    /// </summary>
    Task<IEnumerable<OutboxEvent>> GetFailedAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Add event to outbox.
    /// Called during command handler SaveChangesWithEventPublishingAsync.
    /// </summary>
    Task AddAsync(OutboxEvent @event, CancellationToken cancellationToken = default);

    /// <summary>
    /// Mark event as published.
    /// Updates PublishedAt timestamp and IsPublished flag.
    /// Called after successful Kafka publish.
    /// </summary>
    Task MarkAsPublishedAsync(Guid eventId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Increment failure count on event.
    /// Updates ErrorMessage for debugging.
    /// Called when event publishing fails.
    /// </summary>
    Task IncrementAttemptAsync(
        Guid eventId,
        string failureReason,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get event by ID.
    /// </summary>
    Task<OutboxEvent?> GetByIdAsync(Guid eventId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete published events older than specified days.
    /// Prevents outbox table from growing indefinitely.
    /// </summary>
    Task DeletePublishedOlderThanAsync(int days, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get count of pending events.
    /// Used for monitoring.
    /// </summary>
    Task<int> GetPendingCountAsync(CancellationToken cancellationToken = default);
}
