namespace EHRPlatform.Services.Notification.Domain.Entities;

/// <summary>
/// Notification - Multi-channel notification with retry logic.
/// Channels: Email, SMS, Push, InApp
/// Status: Pending → Sent / Failed → Retrying
/// </summary>
public class Notification
{
    public Guid Id { get; set; }
    public Guid RecipientId { get; set; }
    public string Channel { get; set; } = string.Empty; // Email, SMS, Push, InApp
    public string NotificationType { get; set; } = string.Empty; // EmailVerification, PasswordReset, AppointmentReminder, etc.
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string Status { get; set; } = "Pending"; // Pending, Sent, Failed, Bounced, Unsubscribed
    
    // Retry tracking
    public int RetryCount { get; set; } = 0;
    public int MaxRetries { get; set; } = 3;
    public DateTime? NextRetryAt { get; set; }
    
    // Scheduling
    public DateTime? ScheduledFor { get; set; }
    public DateTime? SentAt { get; set; }
    public string? MessageId { get; set; } // Provider-specific ID (SendGrid, Twilio, FCM)
    
    // Error tracking
    public string? FailureReason { get; set; }
    public DateTime? FailedAt { get; set; }
    
    // Template variables for rendering
    public string? TemplateVariables { get; set; } // JSON: {name: "John", resetLink: "..."}
    
    // Metadata
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    private readonly List<object> _domainEvents = new();

    public void Send(string messageId)
    {
        Status = "Sent";
        MessageId = messageId;
        SentAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
        RetryCount = 0;
        NextRetryAt = null;
        RaiseEvent(new NotificationSentEvent(Id, RecipientId, Channel, NotificationType));
    }

    public void MarkFailed(string reason)
    {
        FailureReason = reason;
        FailedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;

        // Determine if we should retry
        if (RetryCount < MaxRetries)
        {
            Status = "Pending"; // Will be retried
            RetryCount++;
            // Exponential backoff: 5 min, 15 min, 1 hour
            var delayMinutes = RetryCount switch
            {
                1 => 5,
                2 => 15,
                3 => 60,
                _ => 5
            };
            NextRetryAt = DateTime.UtcNow.AddMinutes(delayMinutes);
            RaiseEvent(new NotificationRetryScheduledEvent(Id, RecipientId, RetryCount, NextRetryAt.Value));
        }
        else
        {
            Status = "Failed";
            RaiseEvent(new NotificationFailedEvent(Id, RecipientId, Channel, reason));
        }
    }

    public void MarkBounced()
    {
        Status = "Bounced";
        UpdatedAt = DateTime.UtcNow;
        RaiseEvent(new NotificationBouncedEvent(Id, RecipientId, Channel));
    }

    public void MarkUnsubscribed()
    {
        Status = "Unsubscribed";
        UpdatedAt = DateTime.UtcNow;
    }

    public bool IsReadyToSend()
    {
        return Status == "Pending" &&
               (ScheduledFor == null || ScheduledFor <= DateTime.UtcNow);
    }

    public bool IsReadyToRetry()
    {
        return Status == "Pending" &&
               NextRetryAt != null &&
               NextRetryAt <= DateTime.UtcNow &&
               RetryCount < MaxRetries;
    }

    public void RaiseEvent(object @event) => _domainEvents.Add(@event);
    public IReadOnlyList<object> GetDomainEvents() => _domainEvents.AsReadOnly();
    public void ClearDomainEvents() => _domainEvents.Clear();
}

/// <summary>
/// NotificationPreference - User notification preferences
/// </summary>
public class NotificationPreference
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Channel { get; set; } = string.Empty; // Email, SMS, Push, InApp
    public string NotificationType { get; set; } = string.Empty; // EmailVerification, PasswordReset, etc.
    public bool IsEnabled { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

// Domain Events
public record NotificationSentEvent(Guid NotificationId, Guid RecipientId, string Channel, string NotificationType)
{
    public DateTime CreatedAt { get; } = DateTime.UtcNow;
}

public record NotificationFailedEvent(Guid NotificationId, Guid RecipientId, string Channel, string Reason)
{
    public DateTime CreatedAt { get; } = DateTime.UtcNow;
}

public record NotificationBouncedEvent(Guid NotificationId, Guid RecipientId, string Channel)
{
    public DateTime CreatedAt { get; } = DateTime.UtcNow;
}

public record NotificationRetryScheduledEvent(Guid NotificationId, Guid RecipientId, int RetryCount, DateTime NextRetryAt)
{
    public DateTime CreatedAt { get; } = DateTime.UtcNow;
}
