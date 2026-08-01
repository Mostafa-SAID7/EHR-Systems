using System;
using System.Threading;
using System.Threading.Tasks;

namespace EHRPlatform.EventBus.Outbox;

/// <summary>
/// Interface for publishing outbox messages to message broker.
/// Single responsibility: Publish outbox events to external broker.
/// </summary>
public interface IOutboxMessagePublisher
{
    /// <summary>
    /// Publish outbox event to message broker.
    /// </summary>
    Task PublishAsync(OutboxEventData eventData, CancellationToken cancellationToken = default);

    /// <summary>
    /// Publish with retry logic.
    /// </summary>
    Task PublishWithRetryAsync(OutboxEventData eventData, int maxRetries = 3, CancellationToken cancellationToken = default);

    /// <summary>
    /// Batch publish multiple events.
    /// </summary>
    Task<int> PublishBatchAsync(OutboxEventData[] events, CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if publisher is connected.
    /// </summary>
    Task<bool> IsConnectedAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get publisher health status.
    /// </summary>
    Task<PublisherHealthStatus> GetHealthAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Publisher health status enumeration.
/// Single responsibility: Health status values.
/// </summary>
public enum PublisherHealthStatus
{
    /// <summary>
    /// Publisher is healthy.
    /// </summary>
    Healthy = 0,

    /// <summary>
    /// Publisher is degraded.
    /// </summary>
    Degraded = 1,

    /// <summary>
    /// Publisher is unhealthy.
    /// </summary>
    Unhealthy = 2
}
