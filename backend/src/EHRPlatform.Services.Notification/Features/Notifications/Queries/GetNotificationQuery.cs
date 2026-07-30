using EHRPlatform.Common.Application.CQRS;
using EHRPlatform.Services.Notification.Features.Notifications.Dtos.Responses;

namespace EHRPlatform.Services.Notification.Features.Notifications.Queries;

/// <summary>
/// Get notification by ID - CACHED query.
/// </summary>
public record GetNotificationQuery : ICachedQuery<NotificationResponseDto>
{
    public Guid NotificationId { get; init; }

    public string CacheKey => $"notification_{NotificationId}";
    public int CacheDurationSeconds => 600;
}

/// <summary>
/// Get user notifications - CACHED query.
/// </summary>
public record GetUserNotificationsQuery : ICachedQuery<NotificationListDto>
{
    public Guid UserId { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 50;

    public string CacheKey => $"notifications_user_{UserId}_{PageNumber}_{PageSize}";
    public int CacheDurationSeconds => 300;
}

/// <summary>
/// Get user notification preferences - CACHED query.
/// </summary>
public record GetUserPreferencesQuery : ICachedQuery<List<PreferenceDto>>
{
    public Guid UserId { get; init; }

    public string CacheKey => $"preferences_user_{UserId}";
    public int CacheDurationSeconds => 900;
}

