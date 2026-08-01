using System;

namespace EHRPlatform.Common.Shared.DTOs
{
    /// <summary>
    /// Shared DTO for Notification Communication
    /// </summary>
    public class NotificationDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Type { get; set; }          // e.g., "Email", "SMS", "Push"
        public string Subject { get; set; }
        public string Message { get; set; }
        public string Status { get; set; }        // e.g., "Pending", "Sent", "Failed"
        public DateTime CreatedAt { get; set; }
        public DateTime? SentAt { get; set; }
    }

    /// <summary>
    /// Event: Email Notification Sent
    /// Published by Notification Service after email is sent
    /// Subscribed by: Audit (track communications)
    /// </summary>
    public class EmailNotificationSentEvent
    {
        public Guid NotificationId { get; set; }
        public Guid UserId { get; set; }
        public string RecipientEmail { get; set; }
        public string Subject { get; set; }
        public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Event: SMS Notification Sent
    /// Published by Notification Service after SMS is sent
    /// Subscribed by: Audit (track communications)
    /// </summary>
    public class SmsNotificationSentEvent
    {
        public Guid NotificationId { get; set; }
        public Guid UserId { get; set; }
        public string PhoneNumber { get; set; }
        public string Message { get; set; }
        public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Event: Notification Failed
    /// Published by Notification Service when notification sending fails
    /// Subscribed by: Audit (track failures)
    /// </summary>
    public class NotificationFailedEvent
    {
        public Guid NotificationId { get; set; }
        public Guid UserId { get; set; }
        public string NotificationType { get; set; }
        public string Reason { get; set; }
        public int RetryCount { get; set; }
        public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
    }
}
