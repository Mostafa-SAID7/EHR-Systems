using EHRPlatform.Common.Domain.Events;
using EHRPlatform.Common.Application.Features.EventDriven.Outbox;
using EHRPlatform.Common.Messaging;
using Microsoft.Extensions.Logging;

namespace EHRPlatform.Common.Application.Features.EventDriven.Publishing;

/// <summary>
/// Publishes outbox events to message broker.
/// Single responsibility: Publishing logic only.
/// </summary>
public class OutboxEventPublisher : IOutboxEventPublisher
{
    private readonly IEventPublisher _eventPublisher;
    private readonly IOutboxRepository _outboxRepository;
    private readonly IRetryPolicy _retryPolicy;
    private readonly ILogger<OutboxEventPublisher> _logger;

    public OutboxEventPublisher(
        IEventPublisher eventPublisher,
        IOutboxRepository outboxRepository,
        IRetryPolicy retryPolicy,
        ILogger<OutboxEventPublisher> logger)
    {
        _eventPublisher = eventPublisher ?? throw new ArgumentNullException(nameof(eventPublisher));
        _outboxRepository = outboxRepository ?? throw new ArgumentNullException(nameof(outboxRepository));
        _retryPolicy = retryPolicy ?? throw new ArgumentNullException(nameof(retryPolicy));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task PublishAsync(OutboxEvent outboxEvent, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug(
                "Publishing outbox event {EventId} ({EventType})",
                outboxEvent.Id, outboxEvent.EventType);

            // Wrap stored event in publishable envelope
            var envelope = new OutboxPublishableEvent(
                outboxEvent.Id,
                outboxEvent.EventType,
                outboxEvent.CreatedAt);

            await _eventPublisher.PublishAsync(envelope, cancellationToken);
            await _outboxRepository.MarkAsPublishedAsync(outboxEvent.Id, cancellationToken);

            _logger.LogInformation("Outbox event {EventId} published successfully", outboxEvent.Id);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                ex,
                "Failed to publish outbox event {EventId} (attempt {Attempt}/{Max})",
                outboxEvent.Id,
                outboxEvent.PublishAttempts + 1,
                outboxEvent.MaxPublishAttempts);

            await _outboxRepository.IncrementAttemptAsync(outboxEvent.Id, ex.Message, cancellationToken);

            if (!_retryPolicy.ShouldRetry(outboxEvent.PublishAttempts + 1, outboxEvent.MaxPublishAttempts))
            {
                _logger.LogError(
                    "Outbox event {EventId} exceeded max retries – consider dead-letter queue",
                    outboxEvent.Id);
            }
        }
    }

    public async Task PublishBatchAsync(IEnumerable<OutboxEvent> events, CancellationToken cancellationToken = default)
    {
        var eventList = events.ToList();
        _logger.LogInformation("Publishing batch of {Count} outbox events", eventList.Count);

        foreach (var @event in eventList)
        {
            await PublishAsync(@event, cancellationToken);
        }
    }
}
