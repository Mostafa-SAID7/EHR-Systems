using EHRPlatform.Common.Events;

namespace EHRPlatform.Common.Messaging;

/// <summary>
/// Abstraction for publishing integration events to a messaging bus (e.g. Kafka).
/// </summary>
public interface IEventPublisher
{
    /// <summary>Publish a single integration event.</summary>
    Task PublishAsync(IntegrationEvent @event, CancellationToken cancellationToken = default);

    /// <summary>Publish multiple integration events in a single batch.</summary>
    Task PublishBatchAsync(
        IEnumerable<IntegrationEvent> events,
        CancellationToken cancellationToken = default);
}
