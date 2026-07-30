using System.Text.Json;
using EHRPlatform.Common.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EHRPlatform.Common.Messaging;

/// <summary>
/// Background service that polls the outbox table and publishes pending events to Kafka.
///
/// Flow:
///   1. Poll for unpublished <see cref="OutboxEvent"/> rows.
///   2. Publish each to Kafka via <see cref="IEventPublisher"/>.
///   3. On success → mark as published.
///   4. On failure → increment retry count.
///   5. Events exceeding max retries are left for dead-letter processing.
///
/// HIPAA: Events are durable in the database until published; no events are lost on restart.
/// </summary>
public sealed class OutboxProcessor : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<OutboxProcessor> _logger;
    private const int PollIntervalSeconds = 5;
    private const int CleanupDays = 30;

    public OutboxProcessor(IServiceProvider serviceProvider, ILogger<OutboxProcessor> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Outbox processor starting");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessBatchAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in outbox processor loop");
            }

            await Task.Delay(TimeSpan.FromSeconds(PollIntervalSeconds), stoppingToken);
        }

        _logger.LogInformation("Outbox processor stopped");
    }

    private async Task ProcessBatchAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var outboxRepo = scope.ServiceProvider.GetRequiredService<IOutboxRepository>();
        var publisher  = scope.ServiceProvider.GetRequiredService<IEventPublisher>();

        var pending = (await outboxRepo.GetUnpublishedAsync(cancellationToken)).ToList();

        if (pending.Count > 0)
        {
            _logger.LogInformation("Processing {Count} outbox events", pending.Count);

            foreach (var outboxEvent in pending)
                await TryPublishAsync(outboxEvent, outboxRepo, publisher, cancellationToken);
        }

        await outboxRepo.DeletePublishedOlderThanAsync(CleanupDays, cancellationToken);

        var stillPending = await outboxRepo.GetPendingCountAsync(cancellationToken);
        if (stillPending > 0)
            _logger.LogInformation("Outbox has {Count} pending events", stillPending);
    }

    private async Task TryPublishAsync(
        OutboxEvent outboxEvent,
        IOutboxRepository outboxRepo,
        IEventPublisher publisher,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogDebug(
                "Publishing outbox event {EventId} ({EventType})",
                outboxEvent.Id, outboxEvent.EventType);

            // Wrap the stored event data in a publishable envelope.
            var envelope = new OutboxPublishableEvent(
                outboxEvent.Id,
                outboxEvent.EventType,
                outboxEvent.CreatedAt);

            await publisher.PublishAsync(envelope, cancellationToken);
            await outboxRepo.MarkAsPublishedAsync(outboxEvent.Id, cancellationToken);

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

            await outboxRepo.IncrementAttemptAsync(outboxEvent.Id, ex.Message, cancellationToken);

            if (outboxEvent.PublishAttempts + 1 >= outboxEvent.MaxPublishAttempts)
            {
                _logger.LogError(
                    "Outbox event {EventId} exceeded max retries – consider dead-letter queue",
                    outboxEvent.Id);
            }
        }
    }
}
