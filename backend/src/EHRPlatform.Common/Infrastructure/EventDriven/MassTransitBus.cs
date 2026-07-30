using EHRPlatform.Common.Events;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace EHRPlatform.Common.Messaging;

/// <summary>
/// MassTransit-backed implementation of <see cref="IMessageBus"/>.
/// Routes domain events to Kafka and background jobs to RabbitMQ via separate bus instances.
/// Named EHRMessageBus to avoid conflict with MassTransit.MassTransitBus.
/// </summary>
public sealed class EHRMessageBus : IMessageBus
{
    private readonly IBus _bus;
    private readonly ILogger<EHRMessageBus> _logger;

    public EHRMessageBus(IBus bus, ILogger<EHRMessageBus> logger)
    {
        _bus = bus ?? throw new ArgumentNullException(nameof(bus));
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task PublishDomainEventAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : IntegrationEvent
    {
        _logger.LogDebug(
            "Publishing domain event {EventType} ({EventId}) via MassTransit",
            @event.EventType, @event.EventId);

        await _bus.Publish(@event, cancellationToken);

        _logger.LogInformation(
            "Domain event {EventType} ({EventId}) published successfully",
            @event.EventType, @event.EventId);
    }

    /// <inheritdoc/>
    public async Task SendBackgroundJobAsync<TMessage>(TMessage message, CancellationToken cancellationToken = default)
        where TMessage : class
    {
        _logger.LogDebug("Sending background job {MessageType} to bus", typeof(TMessage).Name);

        // MassTransit's Send requires an address; use Publish so RabbitMQ topology routes it.
        await _bus.Publish(message, cancellationToken);

        _logger.LogInformation("Background job {MessageType} sent", typeof(TMessage).Name);
    }

    /// <inheritdoc/>
    public async Task SendBackgroundJobAsync<TMessage>(TMessage message, Uri queueAddress, CancellationToken cancellationToken = default)
        where TMessage : class
    {
        _logger.LogDebug(
            "Sending background job {MessageType} to {Queue}",
            typeof(TMessage).Name, queueAddress);

        var endpoint = await _bus.GetSendEndpoint(queueAddress);
        await endpoint.Send(message, cancellationToken);

        _logger.LogInformation(
            "Background job {MessageType} sent to {Queue}",
            typeof(TMessage).Name, queueAddress);
    }

    /// <inheritdoc/>
    public async Task<TResponse> RequestAsync<TRequest, TResponse>(TRequest request, CancellationToken cancellationToken = default)
        where TRequest : class
        where TResponse : class
    {
        _logger.LogDebug("Sending request {RequestType}", typeof(TRequest).Name);

        var client = _bus.CreateRequestClient<TRequest>();
        var response = await client.GetResponse<TResponse>(request, cancellationToken);
        return response.Message;
    }
}
