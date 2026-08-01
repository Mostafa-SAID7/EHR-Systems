namespace EHRPlatform.Services.Notification.Application.Features.Notifications.Commands;

using MediatR;

/// <summary>
/// Mark notification as read
/// </summary>
public record MarkAsReadCommand(
    Guid NotificationId) : IRequest<MarkAsReadResponse>;

/// <summary>
/// Response from marking notification as read
/// </summary>
public record MarkAsReadResponse(
    bool Success,
    string Message);
