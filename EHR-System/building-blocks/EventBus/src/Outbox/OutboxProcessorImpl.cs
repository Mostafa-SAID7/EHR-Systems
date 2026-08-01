using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace EHRPlatform.EventBus.Outbox;

/// <summary>
/// Default outbox processor implementation.
/// Single responsibility: Orchestrating outbox message processing.
/// </summary>
public abstract class OutboxProcessorImpl : IOutboxProcessor
{
    protected readonly ILogger Logger;

    protected OutboxProcessorImpl(ILogger logger)
    {
        Logger = logger;
    }

    /// <summary>
    /// Process unpublished outbox messages.
    /// </summary>
    public async Task ProcessUnpublishedMessagesAsync(CancellationToken cancellationToken)
    {
        var unpublishedMessages = await GetUnpublishedMessagesAsync(cancellationToken);

        if (unpublishedMessages.Count == 0)
            return;

        Logger.LogInformation($"Processing {unpublishedMessages.Count} unpublished outbox messages");

        foreach (var message in unpublishedMessages)
        {
            var state = new OutboxMessageState(message);

            if (!state.ShouldRetry())
            {
                Logger.LogWarning(
                    $"Outbox message {message.Id} exceeded max retry attempts. Last error: {message.Error}");
                continue;
            }

            try
            {
                await PublishMessageAsync(message, cancellationToken);
                state.MarkAsPublished();
                await MarkAsPublishedAsync(message.Id, cancellationToken);
                Logger.LogInformation(
                    $"Published outbox message {message.Id} ({message.EventType})");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex,
                    $"Failed to publish outbox message {message.Id}. Attempt {message.PublishAttempts + 1}/{message.MaxPublishAttempts}");
                state.RecordFailedAttempt(ex.Message);
                await UpdateFailedAttemptAsync(message.Id, ex.Message, cancellationToken);
            }
        }
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
}
