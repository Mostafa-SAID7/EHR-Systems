using EHRPlatform.BuildingBlocks.Common.Events;

namespace EHRPlatform.Services.Notification.Domain.Events;

/// <summary>
/// Domain event raised when a notification fails to send after max retries.
/// </summary>
public class NotificationFailedEvent : IntegrationEvent
{
    public Guid NotificationId { get; set; }
    public Guid RecipientId { get; set; }
    public string Channel { get; set; }
    public string Reason { get; set; }

    public NotificationFailedEvent(Guid id, Guid recipientId, string channel, string reason)
    {
        NotificationId = id;
        RecipientId = recipientId;
        Channel = channel;
        Reason = reason;
    }
}

