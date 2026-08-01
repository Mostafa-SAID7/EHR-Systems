using System.Threading;
using System.Threading.Tasks;

namespace EHRPlatform.EventBus.Handlers;

/// <summary>
/// Handler for integration events across service boundaries.
/// </summary>
/// <typeparam name="TEvent">Integration event type to handle.</typeparam>
public interface IIntegrationEventHandler<in TEvent>
    where TEvent : Events.IntegrationEvent
{
    /// <summary>
    /// Handle the integration event.
    /// </summary>
    Task HandleAsync(TEvent @event, CancellationToken cancellationToken);
}
