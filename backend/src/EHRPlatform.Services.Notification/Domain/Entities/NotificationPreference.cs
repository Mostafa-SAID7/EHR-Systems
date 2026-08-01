using EHRPlatform.BuildingBlocks.SharedKernel.Entities;

namespace EHRPlatform.Services.Notification.Domain.Entities;

/// <summary>
/// User notification preferences (opt-in/out).
/// </summary>
public class NotificationPreference : BaseEntity
{
    public Guid UserId { get; set; }
    public string Channel { get; set; } = string.Empty;
    public string NotificationType { get; set; } = string.Empty;
    public bool IsEnabled { get; set; } = true;
}


