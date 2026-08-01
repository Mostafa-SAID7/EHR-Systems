using System.Threading;
using System.Threading.Tasks;
using EHRPlatform.EventBus.Events;

namespace EHRPlatform.EventBus.Publishers;

/// <summary>
/// Publisher for domain events within a service (via MediatR).
/// Single responsibility: Publishing domain events locally.
/// </summary>
public interface IEventPublisher
{
    /// <summary>
    /// Publish a domain event.
    /// </summary>
    Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : DomainEvent;
}
