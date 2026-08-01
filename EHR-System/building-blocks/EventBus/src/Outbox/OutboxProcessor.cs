using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EHRPlatform.EventBus.Outbox;

/// <summary>
/// Background service that processes outbox messages.
/// 
/// Runs continuously, reading unpublished outbox entries and publishing to message broker.
/// Ensures guaranteed delivery of domain events across services.
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
    /// Main processing loop - reads and publishes unpublished outbox messages.
    /// </summary>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Logger.LogInformation("Outbox processor started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessUnpublishedMessagesAsync(stoppingToken);
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

    /// <summary>
    /// Process all unpublished outbox messages.
    /// </summary>
    private async Task ProcessUnpublishedMessagesAsync(CancellationToken cancellationToken)
    {
        var unpublishedMessages = await GetUnpublishedMessagesAsync(cancellationToken);

        if (unpublishedMessages.Count == 0)
            return;

        Logger.LogInformation($"Processing {unpublishedMessages.Count} unpublished outbox messages");

        foreach (var message in unpublishedMessages)
        {
            if (!message.ShouldRetry())
            {
                Logger.LogWarning(
                    $"Outbox message {message.Id} exceeded max retry attempts. Last error: {message.Error}");
                continue;
            }

            try
            {
                await PublishMessageAsync(message, cancellationToken);
                await MarkAsPublishedAsync(message.Id, cancellationToken);
                Logger.LogInformation(
                    $"Published outbox message {message.Id} ({message.EventType})");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex,
                    $"Failed to publish outbox message {message.Id}. Attempt {message.PublishAttempts + 1}/{message.MaxPublishAttempts}");
                await UpdateFailedAttemptAsync(message.Id, ex.Message, cancellationToken);
            }
        }
    }
}
