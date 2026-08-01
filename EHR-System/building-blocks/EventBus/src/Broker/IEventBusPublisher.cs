using System.Threading;
using System.Threading.Tasks;

namespace EHRPlatform.EventBus.Broker;

/// <summary>
/// Interface for publishing events to message broker.
/// Single responsibility: Event publishing to external bus.
/// </summary>
public interface IEventBusPublisher
{
    /// <summary>
    /// Publish event to message broker.
    /// </summary>
    Task PublishAsync<T>(T @event, CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Publish multiple events in batch.
    /// </summary>
    Task PublishBatchAsync<T>(T[] events, CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Publish event with delay.
    /// </summary>
    Task PublishDelayedAsync<T>(T @event, int delaySeconds, CancellationToken cancellationToken = default) where T : class;
}
