using EHRPlatform.BuildingBlocks.Common.Application.CQRS;

namespace EHRPlatform.Services.Notification.Features.Notifications.Commands;

/// <summary>
/// Set notification preference command.
/// Single Responsibility: Update user notification opt-in/out preferences.
/// </summary>
public record SetNotificationPreferenceCommand : ICommand
{
    public Guid UserId { get; init; }
    public string Channel { get; init; } = string.Empty;
    public string NotificationType { get; init; } = string.Empty;
    public bool IsEnabled { get; init; }
}


