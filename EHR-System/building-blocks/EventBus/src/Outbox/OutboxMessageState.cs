using System;

namespace EHRPlatform.EventBus.Outbox;

/// <summary>
/// Outbox message state and business logic.
/// Single responsibility: Managing outbox message state transitions and retry logic.
/// </summary>
public class OutboxMessageState
{
    private readonly OutboxMessage _message;

    public OutboxMessageState(OutboxMessage message)
    {
        _message = message ?? throw new ArgumentNullException(nameof(message));
    }

    /// <summary>
    /// Whether this event has been successfully published.
    /// </summary>
    public bool IsPublished => _message.PublishedAt.HasValue;

    /// <summary>
    /// Mark as published.
    /// </summary>
    public void MarkAsPublished()
    {
        _message.PublishedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Record failed publish attempt.
    /// </summary>
    public void RecordFailedAttempt(string error)
    {
        _message.PublishAttempts++;
        _message.Error = error;
    }

    /// <summary>
    /// Check if should retry (max attempts not exceeded).
    /// </summary>
    public bool ShouldRetry()
    {
        return !IsPublished && _message.PublishAttempts < _message.MaxPublishAttempts;
    }

    /// <summary>
    /// Get the underlying message.
    /// </summary>
    public OutboxMessage GetMessage() => _message;
}
