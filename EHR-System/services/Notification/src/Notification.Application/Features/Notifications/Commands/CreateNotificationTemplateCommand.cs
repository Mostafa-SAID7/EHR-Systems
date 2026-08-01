namespace EHRPlatform.Services.Notification.Application.Features.Notifications.Commands;

using MediatR;

/// <summary>
/// Create notification template
/// </summary>
public record CreateNotificationTemplateCommand(
    string TemplateName,
    string Subject,
    string Body,
    string? ContentType = null) : IRequest<CreateNotificationTemplateResponse>;

/// <summary>
/// Response from creating template
/// </summary>
public record CreateNotificationTemplateResponse(
    bool Success,
    string Message,
    Guid? TemplateId = null);
