using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace EHRPlatform.EventBus.Handlers;

/// <summary>
/// Base handler for domain events.
/// Implement this interface to handle domain events within a service.
/// </summary>
/// <typeparam name="TEvent">Domain event type to handle.</typeparam>
public interface IDomainEventHandler<in TEvent> : INotificationHandler<TEvent>
    where TEvent : Events.DomainEvent
{
    /// <summary>
    /// Handle the domain event.
    /// </summary>
    Task HandleAsync(TEvent @event, CancellationToken cancellationToken);
}

/// <summary>
/// Base abstract handler implementation.
/// </summary>
public abstract class DomainEventHandler<TEvent> : IDomainEventHandler<TEvent>
    where TEvent : Events.DomainEvent
{
    public async Task Handle(TEvent @event, CancellationToken cancellationToken)
    {
        await HandleAsync(@event, cancellationToken);
    }

    public abstract Task HandleAsync(TEvent @event, CancellationToken cancellationToken);
}
