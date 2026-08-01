using EHRPlatform.BuildingBlocks.Common.Events;

namespace EHRPlatform.Services.Notification.Domain.Events;

/// <summary>
/// Domain event raised when a notification is successfully sent.
/// </summary>
public class NotificationSentEvent : IntegrationEvent
{
    public Guid NotificationId { get; set; }
    public Guid RecipientId { get; set; }
    public string Channel { get; set; }
    public string NotificationType { get; set; }

    public NotificationSentEvent(Guid id, Guid recipientId, string channel, string type)
    {
        NotificationId = id;
        RecipientId = recipientId;
        Channel = channel;
        NotificationType = type;
    }
}

