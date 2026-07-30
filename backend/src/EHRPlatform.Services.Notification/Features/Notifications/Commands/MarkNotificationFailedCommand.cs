using EHRPlatform.Common.Application.CQRS;

namespace EHRPlatform.Services.Notification.Features.Notifications.Commands;

/// <summary>
/// Mark notification failed command.
/// Single Responsibility: Signal delivery failure with retry logic.
/// </summary>
public record MarkNotificationFailedCommand : ICommand
{
    public Guid NotificationId { get; init; }
    public string Reason { get; init; } = string.Empty;
}

