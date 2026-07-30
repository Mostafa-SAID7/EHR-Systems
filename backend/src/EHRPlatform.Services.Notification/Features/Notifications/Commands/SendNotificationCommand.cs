using EHRPlatform.Common.Application.CQRS;
using EHRPlatform.Services.Notification.Features.Notifications.Dtos.Responses;

namespace EHRPlatform.Services.Notification.Features.Notifications.Commands;

/// <summary>
/// Send notification command.
/// </summary>
public record SendNotificationCommand : ICommand<NotificationResponseDto>
{
    public Guid RecipientId { get; init; }
    public string Channel { get; init; } = string.Empty; // Email, SMS, Push, InApp
    public string NotificationType { get; init; } = string.Empty;
    public string Subject { get; init; } = string.Empty;
    public string Body { get; init; } = string.Empty;
    public Dictionary<string, string>? TemplateVars { get; init; }
    public string? Recipient { get; init; } // Email, phone, device token
    public DateTime? ScheduledFor { get; init; }
}

