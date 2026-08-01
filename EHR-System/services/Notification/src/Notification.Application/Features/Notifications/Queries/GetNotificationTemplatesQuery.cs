namespace EHRPlatform.Services.Notification.Application.Features.Notifications.Queries;

using MediatR;

/// <summary>
/// Get all notification templates
/// </summary>
public record GetNotificationTemplatesQuery(
    int PageNumber = 1,
    int PageSize = 20) : IRequest<GetNotificationTemplatesResponse>;

/// <summary>
/// Response with notification templates
/// </summary>
public record GetNotificationTemplatesResponse(
    bool Success,
    string Message,
    IEnumerable<NotificationTemplateDto> Templates,
    int TotalCount,
    int PageNumber,
    int PageSize);

/// <summary>
/// Notification template DTO
/// </summary>
public record NotificationTemplateDto(
    Guid Id,
    string TemplateName,
    string Subject,
    string Body,
    DateTime CreatedAt);
