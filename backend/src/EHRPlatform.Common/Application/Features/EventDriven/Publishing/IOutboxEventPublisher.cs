using EHRPlatform.Common.Domain.Events;

namespace EHRPlatform.Common.Application.Features.EventDriven.Publishing;

/// <summary>
/// Abstraction for publishing outbox events.
/// Single responsibility: Define publishing contract.
/// </summary>
public interface IOutboxEventPublisher
{
    /// <summary>
    /// Publish a single outbox event to message broker.
    /// </summary>
    Task PublishAsync(OutboxEvent outboxEvent, CancellationToken cancellationToken = default);

    /// <summary>
    /// Publish multiple outbox events in batch.
    /// </summary>
    Task PublishBatchAsync(IEnumerable<OutboxEvent> events, CancellationToken cancellationToken = default);
}
