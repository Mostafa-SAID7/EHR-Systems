using System.Threading;
using System.Threading.Tasks;

namespace EHRPlatform.EventBus.Handlers;

/// <summary>
/// Base abstract handler implementation for domain events.
/// Single responsibility: Implementing domain event handler pattern.
/// </summary>
public abstract class DomainEventHandler<TEvent>
    where TEvent : Events.DomainEvent
{
    public abstract Task HandleAsync(TEvent @event, CancellationToken cancellationToken);
}
