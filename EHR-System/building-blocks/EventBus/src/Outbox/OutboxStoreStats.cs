using System;

namespace EHRPlatform.EventBus.Outbox;

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
