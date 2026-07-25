using Mapster;
using EHRPlatform.Common.Mapping;

using EHRPlatform.Services.Notification.Features.Notifications.Dtos.Responses;
using Microsoft.Extensions.Logging;

namespace EHRPlatform.Services.Notification.Application.NotificationManagement.Mappers;

/// <summary>
/// Notification Mapper
/// Single Responsibility: Convert between Notification domain models and DTOs.
/// Handles all Notification-related mappings with optional post-processing.
/// </summary>
public class NotificationMapper : MappingServiceBase<Notification, NotificationResponseDto>
{
    public NotificationMapper(ILogger<NotificationMapper> logger) : base(logger)
    {
    }

    /// <summary>
    /// Map single notification to response DTO.
    /// </summary>
    public NotificationResponseDto MapToResponseDto(Notification notification)
    {
        return MapToDto(notification);
    }

    /// <summary>
    /// Map collection of notifications to response DTO list.
    /// </summary>
    public List<NotificationResponseDto> MapToResponseDtoList(ICollection<Notification> notifications)
    {
        Logger.LogDebug("Mapping {Count} notifications to response DTO list", notifications.Count);
        return notifications.Adapt<List<NotificationResponseDto>>();
    }

    /// <summary>
    /// Map notification template to DTO.
    /// </summary>
    public NotificationTemplateDto MapTemplateToDto(NotificationTemplate template)
    {
        Logger.LogDebug("Mapping notification template {TemplateId} to DTO", template.Id);

        return new NotificationTemplateDto
        {
            Id = template.Id,
            Name = template.Name,
            Channel = template.Channel,
            NotificationType = template.NotificationType,
            Subject = template.Subject,
            BodyTemplate = template.BodyTemplate,
            IsActive = template.IsActive
        };
    }

    /// <summary>
    /// Map collection of templates to DTO list.
    /// </summary>
    public List<NotificationTemplateDto> MapTemplatesToDtoList(ICollection<NotificationTemplate> templates)
    {
        Logger.LogDebug("Mapping {Count} notification templates to DTO list", templates.Count);
        return templates.Adapt<List<NotificationTemplateDto>>();
    }
}
