using System.Threading;
using System.Threading.Tasks;

namespace EHRPlatform.EventBus.Handlers;

/// <summary>
/// Base abstract handler implementation for integration events.
/// Single responsibility: Implementing integration event handler pattern.
/// </summary>
public abstract class IntegrationEventHandler<TEvent> : IIntegrationEventHandler<TEvent>
    where TEvent : Events.IntegrationEvent
{
    public abstract Task HandleAsync(TEvent @event, CancellationToken cancellationToken);
}
