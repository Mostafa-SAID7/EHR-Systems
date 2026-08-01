using System;
using System.Collections.Generic;

namespace EHRPlatform.SharedKernel.EventSourcing;

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
