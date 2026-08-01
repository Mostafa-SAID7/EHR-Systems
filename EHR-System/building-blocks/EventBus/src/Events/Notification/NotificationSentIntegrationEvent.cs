using System;

namespace EHRPlatform.EventBus.Events;

/// <summary>
/// Published when a notification is sent.
/// Consumed by: Audit (compliance logging).
/// Single responsibility: Notification sent event.
/// </summary>
public class NotificationSentIntegrationEvent : IntegrationEvent
{
    public Guid NotificationId { get; set; }
    public Guid RecipientId { get; set; }
    public string Channel { get; set; } = null!;
    public string MessageType { get; set; } = null!;
    public bool WasSuccessful { get; set; }
}
