using EHRPlatform.BuildingBlocks.Common.Events;

namespace EHRPlatform.Services.Notification.Domain.Events;

/// <summary>
/// Domain event raised when a notification is bounced (invalid recipient).
/// </summary>
public class NotificationBouncedEvent : IntegrationEvent
{
    public Guid NotificationId { get; set; }
    public Guid RecipientId { get; set; }
    public string Channel { get; set; }

    public NotificationBouncedEvent(Guid id, Guid recipientId, string channel)
    {
        NotificationId = id;
        RecipientId = recipientId;
        Channel = channel;
    }
}

