using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EHRPlatform.EventBus.Outbox;

/// <summary>
/// Background service for processing outbox messages.
/// Single responsibility: Running outbox processing as background service.
/// </summary>
public abstract class OutboxProcessor : BackgroundService
{
    protected readonly ILogger Logger;
    private readonly int _processingIntervalSeconds;

    public OutboxProcessor(ILogger logger, int processingIntervalSeconds = 5)
    {
        Logger = logger;
        _processingIntervalSeconds = processingIntervalSeconds;
    }

    /// <summary>
    /// Main processing loop.
    /// </summary>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Logger.LogInformation("Outbox processor started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var processor = GetProcessor();
                await processor.ProcessUnpublishedMessagesAsync(stoppingToken);
                await Task.Delay(TimeSpan.FromSeconds(_processingIntervalSeconds), stoppingToken);
            }
            catch (OperationCanceledException)
            {
                Logger.LogInformation("Outbox processor is stopping");
                break;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error processing outbox messages");
                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
            }
        }

        Logger.LogInformation("Outbox processor stopped");
    }

    /// <summary>
    /// Get the outbox processor implementation.
    /// </summary>
    protected abstract IOutboxProcessor GetProcessor();

    /// <summary>
    /// Get unpublished outbox messages from database.
    /// </summary>
    protected abstract Task<List<OutboxMessage>> GetUnpublishedMessagesAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Publish outbox message to message broker.
    /// </summary>
    protected abstract Task PublishMessageAsync(OutboxMessage message, CancellationToken cancellationToken);

    /// <summary>
    /// Mark message as published in database.
    /// </summary>
    protected abstract Task MarkAsPublishedAsync(Guid messageId, CancellationToken cancellationToken);

    /// <summary>
    /// Update failed publish attempt in database.
    /// </summary>
    protected abstract Task UpdateFailedAttemptAsync(Guid messageId, string error, CancellationToken cancellationToken);
}
