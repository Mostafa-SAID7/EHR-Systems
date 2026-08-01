using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace EHRPlatform.EventBus.Handlers;

/// <summary>
/// Handler interface for domain events.
/// Single responsibility: Domain event handling contract.
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
