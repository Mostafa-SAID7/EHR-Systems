using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EHRPlatform.SharedKernel.EventSourcing;

/// <summary>
/// Interface for event store (persisting domain events).
/// Single responsibility: Store and retrieve aggregate events.
/// </summary>
public interface IEventStore
{
    /// <summary>
    /// Append event to aggregate stream.
    /// </summary>
    Task<long> AppendEventAsync(string streamId, object @event, int expectedVersion = -1, CancellationToken cancellationToken = default);

    /// <summary>
    /// Append multiple events to aggregate stream.
    /// </summary>
    Task AppendEventsAsync(string streamId, IEnumerable<object> events, int expectedVersion = -1, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all events for aggregate.
    /// </summary>
    Task<IReadOnlyList<EventEnvelope>> GetEventsAsync(string streamId, int fromVersion = 0, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get events from specific version.
    /// </summary>
    Task<IReadOnlyList<EventEnvelope>> GetEventsSinceAsync(string streamId, int fromVersion, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get events of specific type.
    /// </summary>
    Task<IReadOnlyList<EventEnvelope>> GetEventsByTypeAsync(string streamId, Type eventType, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get current version of aggregate.
    /// </summary>
    Task<int> GetVersionAsync(string streamId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if stream exists.
    /// </summary>
    Task<bool> StreamExistsAsync(string streamId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete stream (soft delete).
    /// </summary>
    Task DeleteStreamAsync(string streamId, CancellationToken cancellationToken = default);
}
