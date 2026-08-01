namespace EHRPlatform.Services.Notification.Application.Features.Notifications.Queries;

using MediatR;

/// <summary>
/// Query to get user notifications (cached).
/// </summary>
public class GetUserNotificationsQuery : IRequest<GetUserNotificationsResponse>
{
    public Guid UserId { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class GetUserNotificationsResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public List<NotificationDto> Notifications { get; set; } = new();
    public int TotalCount { get; set; }
}

public class NotificationDto
{
    public Guid Id { get; set; }
    public string Channel { get; set; } = string.Empty;
    public string NotificationType { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? SentAt { get; set; }
}
