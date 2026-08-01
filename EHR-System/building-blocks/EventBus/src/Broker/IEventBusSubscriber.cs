using System;
using System.Threading;
using System.Threading.Tasks;

namespace EHRPlatform.EventBus.Broker;

/// <summary>
/// Interface for subscribing to events from message broker.
/// Single responsibility: Event subscription from external bus.
/// </summary>
public interface IEventBusSubscriber
{
    /// <summary>
    /// Subscribe to event type with handler.
    /// </summary>
    Task SubscribeAsync<T>(Func<T, CancellationToken, Task> handler, CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Unsubscribe from event type.
    /// </summary>
    Task UnsubscribeAsync<T>(CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Check if subscribed to event type.
    /// </summary>
    Task<bool> IsSubscribedAsync<T>(CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Get all subscription handlers.
    /// </summary>
    Task<int> GetSubscriptionCountAsync(CancellationToken cancellationToken = default);
}
