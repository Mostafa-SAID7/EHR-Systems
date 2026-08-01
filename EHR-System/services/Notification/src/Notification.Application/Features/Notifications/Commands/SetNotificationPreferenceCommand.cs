namespace EHRPlatform.Services.Notification.Application.Features.Notifications.Commands;

using MediatR;

/// <summary>
/// Command to set user notification preferences.
/// </summary>
public class SetNotificationPreferenceCommand : IRequest<SetNotificationPreferenceResponse>
{
    public Guid UserId { get; set; }
    public string Channel { get; set; } = string.Empty; // Email, SMS, Push, InApp
    public string NotificationType { get; set; } = string.Empty;
    public bool IsEnabled { get; set; }
}

public class SetNotificationPreferenceResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
}
