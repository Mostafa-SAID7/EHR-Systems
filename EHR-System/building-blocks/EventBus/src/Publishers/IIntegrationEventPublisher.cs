using System.Threading;
using System.Threading.Tasks;
using EHRPlatform.EventBus.Events;

namespace EHRPlatform.EventBus.Publishers;

/// <summary>
/// Publisher for integration events across service boundaries (via message broker).
/// Single responsibility: Publishing integration events to message broker.
/// </summary>
public interface IIntegrationEventPublisher
{
    /// <summary>
    /// Publish an integration event to message broker.
    /// </summary>
    Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : IntegrationEvent;
}
