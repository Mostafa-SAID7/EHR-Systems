using EHRPlatform.Common.Entities;
using EHRPlatform.Common.Events;
using EHRPlatform.Services.Notification.Domain.Events;

namespace EHRPlatform.Services.Notification.Domain.Entities;

/// <summary>
/// Notification aggregate root.
/// Multi-channel delivery: Email, SMS, Push, In-App.
/// </summary>
public class Notification : AuditableEntity
{
    public Guid RecipientId { get; set; }
    public string Channel { get; set; } = string.Empty; // Email, SMS, Push, InApp
    public string NotificationType { get; set; } = string.Empty; // Appointment, Prescription, Billing, Clinical, System
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string Status { get; set; } = "Pending"; // Pending, Sent, Failed, Bounced, Unsubscribed
    public int RetryCount { get; set; }
    public int MaxRetries { get; set; } = 3;
    public DateTime? ScheduledFor { get; set; }
    public DateTime? SentAt { get; set; }
    public string? FailureReason { get; set; }
    public string? MessageId { get; set; } // Provider message ID (e.g., SES, Twilio)
    public Dictionary<string, string> TemplateVars { get; set; } = new(); // Template variables
    public string? Recipient { get; set; } // Email address, phone, or device token

    private readonly List<IntegrationEvent> _domainEvents = new();

    public void MarkSent(string messageId = "")
    {
        Status = "Sent";
        SentAt = DateTime.UtcNow;
        MessageId = messageId;
        RetryCount = 0;

        RaiseEvent(new NotificationSentEvent(Id, RecipientId, Channel, NotificationType));
    }

    public void MarkFailed(string reason)
    {
        RetryCount++;
        if (RetryCount >= MaxRetries)
        {
            Status = "Failed";
            FailureReason = reason;
            RaiseEvent(new NotificationFailedEvent(Id, RecipientId, Channel, reason));
        }
        else
        {
            // Retry with exponential backoff
            ScheduledFor = DateTime.UtcNow.AddSeconds(Math.Pow(2, RetryCount));
        }
    }

    public void MarkBounced()
    {
        Status = "Bounced";
        RaiseEvent(new NotificationBouncedEvent(Id, RecipientId, Channel));
    }

    public void MarkUnsubscribed()
    {
        Status = "Unsubscribed";
        RaiseEvent(new NotificationUnsubscribedEvent(Id, RecipientId, Channel));
    }

    public void RaiseEvent(IntegrationEvent @event) => _domainEvents.Add(@event);
    public new IReadOnlyList<IntegrationEvent> GetDomainEvents() => _domainEvents.AsReadOnly();
    public new void ClearDomainEvents() => _domainEvents.Clear();
}
