using System;

namespace EHRPlatform.SharedKernel.EventSourcing;

/// <summary>
/// Snapshot data structure.
/// Single responsibility: Aggregate state snapshot.
/// </summary>
public class Snapshot
{
    /// <summary>
    /// Stream ID (aggregate ID).
    /// </summary>
    public string StreamId { get; set; } = null!;

    /// <summary>
    /// Version when snapshot was taken.
    /// </summary>
    public int Version { get; set; }

    /// <summary>
    /// Snapshot state (serialized aggregate).
    /// </summary>
    public object State { get; set; } = null!;

    /// <summary>
    /// Snapshot timestamp.
    /// </summary>
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// Aggregate type name.
    /// </summary>
    public string AggregateType { get; set; } = null!;
}
