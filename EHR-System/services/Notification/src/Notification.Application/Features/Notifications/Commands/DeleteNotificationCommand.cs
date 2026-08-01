namespace EHRPlatform.Services.Notification.Application.Features.Notifications.Commands;

using MediatR;

/// <summary>
/// Delete/archive a notification
/// </summary>
public record DeleteNotificationCommand(
    Guid NotificationId) : IRequest<DeleteNotificationResponse>;

/// <summary>
/// Response from deleting notification
/// </summary>
public record DeleteNotificationResponse(
    bool Success,
    string Message);
