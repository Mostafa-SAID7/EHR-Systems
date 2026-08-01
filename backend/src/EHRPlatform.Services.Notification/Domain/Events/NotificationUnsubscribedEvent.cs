using EHRPlatform.BuildingBlocks.Common.Events;

namespace EHRPlatform.Services.Notification.Domain.Events;

/// <summary>
/// Domain event raised when a recipient unsubscribes from notifications.
/// </summary>
public class NotificationUnsubscribedEvent : IntegrationEvent
{
    public Guid NotificationId { get; set; }
    public Guid RecipientId { get; set; }
    public string Channel { get; set; }

    public NotificationUnsubscribedEvent(Guid id, Guid recipientId, string channel)
    {
        NotificationId = id;
        RecipientId = recipientId;
        Channel = channel;
    }
}

