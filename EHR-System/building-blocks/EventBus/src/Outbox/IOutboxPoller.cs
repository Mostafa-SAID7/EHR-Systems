using System;
using System.Threading;
using System.Threading.Tasks;

namespace EHRPlatform.EventBus.Outbox;

/// <summary>
/// Interface for polling outbox messages and publishing them.
/// Single responsibility: Poll and process outbox messages.
/// </summary>
public interface IOutboxPoller
{
    /// <summary>
    /// Poll outbox for unpublished messages and publish them.
    /// </summary>
    Task<int> PollAndPublishAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Start background polling.
    /// </summary>
    Task StartAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Stop background polling.
    /// </summary>
    Task StopAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Poll specific batch of messages.
    /// </summary>
    Task<int> PollBatchAsync(int batchSize, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get polling statistics.
    /// </summary>
    Task<OutboxPollerStats> GetStatsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Is polling active.
    /// </summary>
    bool IsRunning { get; }
}
