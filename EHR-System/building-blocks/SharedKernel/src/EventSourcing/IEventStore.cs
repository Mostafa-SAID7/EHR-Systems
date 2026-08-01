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

/// <summary>
/// Event envelope (event with metadata).
/// Single responsibility: Event and metadata wrapper.
/// </summary>
public class EventEnvelope
{
    /// <summary>
    /// Event ID.
    /// </summary>
    public string EventId { get; set; } = null!;

    /// <summary>
    /// Stream ID (aggregate ID).
    /// </summary>
    public string StreamId { get; set; } = null!;

    /// <summary>
    /// Event version (sequence number in stream).
    /// </summary>
    public int Version { get; set; }

    /// <summary>
    /// Event type name.
    /// </summary>
    public string EventType { get; set; } = null!;

    /// <summary>
    /// Event payload.
    /// </summary>
    public object Event { get; set; } = null!;

    /// <summary>
    /// Event timestamp.
    /// </summary>
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// User ID who triggered event.
    /// </summary>
    public string? UserId { get; set; }

    /// <summary>
    /// Correlation ID for distributed tracing.
    /// </summary>
    public string? CorrelationId { get; set; }

    /// <summary>
    /// Additional metadata.
    /// </summary>
    public Dictionary<string, string>? Metadata { get; set; }
}
