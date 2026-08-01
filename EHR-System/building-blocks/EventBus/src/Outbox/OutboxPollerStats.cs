using System;

namespace EHRPlatform.EventBus.Outbox;

/// <summary>
/// Outbox poller statistics.
/// Single responsibility: Polling statistics data.
/// </summary>
public class OutboxPollerStats
{
    /// <summary>
    /// Total messages polled.
    /// </summary>
    public long TotalPolled { get; set; }

    /// <summary>
    /// Total messages published.
    /// </summary>
    public long TotalPublished { get; set; }

    /// <summary>
    /// Total polling errors.
    /// </summary>
    public long TotalErrors { get; set; }

    /// <summary>
    /// Last poll time.
    /// </summary>
    public DateTime? LastPollTime { get; set; }

    /// <summary>
    /// Average poll duration in milliseconds.
    /// </summary>
    public double AveragePollDurationMs { get; set; }
}
