using EHRPlatform.BuildingBlocks.Common.Application.CQRS;

namespace EHRPlatform.Services.Notification.Features.Notifications.Commands;

/// <summary>
/// Mark notification sent command.
/// Single Responsibility: Signal successful delivery.
/// </summary>
public record MarkNotificationSentCommand : ICommand
{
    public Guid NotificationId { get; init; }
    public string? MessageId { get; init; }
}


