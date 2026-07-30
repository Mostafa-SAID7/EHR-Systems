using EHRPlatform.Common.Events;

namespace EHRPlatform.Common.Messaging;

/// <summary>
/// Unified message bus abstraction over Kafka (domain events) and RabbitMQ (background jobs).
/// Use <see cref="PublishDomainEventAsync"/> for high-throughput clinical events routed via Kafka.
/// Use <see cref="SendBackgroundJobAsync"/> for task-queue style work routed via RabbitMQ.
/// </summary>
public interface IMessageBus
{
    /// <summary>
    /// Publish a domain integration event to Kafka.
    /// Consumers subscribed to the topic receive this event in order.
    /// </summary>
    Task PublishDomainEventAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : IntegrationEvent;

    /// <summary>
    /// Send a background job message to a RabbitMQ queue.
    /// Exactly one consumer processes the message.
    /// </summary>
    Task SendBackgroundJobAsync<TMessage>(TMessage message, CancellationToken cancellationToken = default)
        where TMessage : class;

    /// <summary>
    /// Send a background job to a specific queue address (for targeted routing).
    /// </summary>
    Task SendBackgroundJobAsync<TMessage>(TMessage message, Uri queueAddress, CancellationToken cancellationToken = default)
        where TMessage : class;

    /// <summary>
    /// Request/response over the bus (synchronous RPC pattern over messaging).
    /// </summary>
    Task<TResponse> RequestAsync<TRequest, TResponse>(TRequest request, CancellationToken cancellationToken = default)
        where TRequest : class
        where TResponse : class;
}
