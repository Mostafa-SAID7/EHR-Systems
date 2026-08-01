namespace EHRPlatform.Services.Notification.Application.Features.Notifications.Commands;

using MediatR;

/// <summary>
/// Command to send notification via specified channel.
/// </summary>
public class SendNotificationCommand : IRequest<SendNotificationResponse>
{
    public Guid RecipientId { get; set; }
    public string Channel { get; set; } = string.Empty; // Email, SMS, Push, InApp
    public string NotificationType { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public DateTime? ScheduledFor { get; set; }
    public Dictionary<string, object>? TemplateVariables { get; set; }
}

public class SendNotificationResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public Guid? NotificationId { get; set; }
}
