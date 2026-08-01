using System;
using System.Threading;
using System.Threading.Tasks;

namespace EHRPlatform.SharedKernel.EventSourcing;

/// <summary>
/// Interface for snapshot store (caching aggregate state).
/// Single responsibility: Store and retrieve aggregate snapshots.
/// </summary>
public interface ISnapshotStore
{
    /// <summary>
    /// Save snapshot of aggregate state.
    /// </summary>
    Task SaveSnapshotAsync(string streamId, int version, object state, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get latest snapshot for aggregate.
    /// </summary>
    Task<Snapshot?> GetLatestSnapshotAsync(string streamId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get snapshot at specific version.
    /// </summary>
    Task<Snapshot?> GetSnapshotAtVersionAsync(string streamId, int version, CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete snapshot.
    /// </summary>
    Task DeleteSnapshotAsync(string streamId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete snapshots older than version.
    /// </summary>
    Task DeleteSnapshotsOlderThanAsync(string streamId, int version, CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if snapshot exists.
    /// </summary>
    Task<bool> SnapshotExistsAsync(string streamId, CancellationToken cancellationToken = default);
}

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
