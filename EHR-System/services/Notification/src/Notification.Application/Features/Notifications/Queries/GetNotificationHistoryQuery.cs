namespace EHRPlatform.Services.Notification.Application.Features.Notifications.Queries;

using MediatR;

/// <summary>
/// Get notification history for a user with optional filters
/// </summary>
public record GetNotificationHistoryQuery(
    Guid UserId,
    string? Type = null,
    DateTime? FromDate = null,
    DateTime? ToDate = null,
    int PageNumber = 1,
    int PageSize = 20) : IRequest<GetNotificationHistoryResponse>;

/// <summary>
/// Response with notification history
/// </summary>
public record GetNotificationHistoryResponse(
    bool Success,
    string Message,
    IEnumerable<NotificationHistoryDto> Notifications,
    int TotalCount,
    int PageNumber,
    int PageSize);

/// <summary>
/// Notification history item
/// </summary>
public record NotificationHistoryDto(
    Guid Id,
    string Title,
    string Type,
    DateTime CreatedAt,
    DateTime? ReadAt,
    bool IsRead);
