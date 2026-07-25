using Mapster;

using EHRPlatform.Services.Notification.Features.Notifications.Dtos.Responses;

namespace EHRPlatform.Services.Notification.Application.NotificationManagement.Mappers;

/// <summary>
/// Mapster registration profile for Notification entity mappings.
/// Handles conversion between domain models and DTOs.
/// Single Responsibility: Configure all Notification-related type mappings.
/// </summary>
public class NotificationMappingProfile : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        // Notification → NotificationResponseDto
        config.NewConfig<Notification, NotificationResponseDto>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.RecipientId, src => src.RecipientId)
            .Map(dest => dest.Channel, src => src.Channel)
            .Map(dest => dest.NotificationType, src => src.NotificationType)
            .Map(dest => dest.Subject, src => src.Subject)
            .Map(dest => dest.Body, src => src.Body)
            .Map(dest => dest.Status, src => src.Status)
            .Map(dest => dest.RetryCount, src => src.RetryCount)
            .Map(dest => dest.ScheduledFor, src => src.ScheduledFor)
            .Map(dest => dest.SentAt, src => src.SentAt)
            .Map(dest => dest.MessageId, src => src.MessageId)
            .Map(dest => dest.Recipient, src => src.Recipient)
            .Map(dest => dest.CreatedAt, src => src.CreatedAt);

        // NotificationTemplate → NotificationTemplateDto
        config.NewConfig<NotificationTemplate, NotificationTemplateDto>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.Name, src => src.Name)
            .Map(dest => dest.Channel, src => src.Channel)
            .Map(dest => dest.NotificationType, src => src.NotificationType)
            .Map(dest => dest.Subject, src => src.Subject)
            .Map(dest => dest.BodyTemplate, src => src.BodyTemplate)
            .Map(dest => dest.IsActive, src => src.IsActive);

        // NotificationResponseDto → Notification (for updates)
        config.NewConfig<NotificationResponseDto, Notification>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.RecipientId, src => src.RecipientId)
            .Map(dest => dest.Channel, src => src.Channel)
            .Map(dest => dest.NotificationType, src => src.NotificationType)
            .Map(dest => dest.Subject, src => src.Subject)
            .Map(dest => dest.Body, src => src.Body)
            .Map(dest => dest.Status, src => src.Status)
            .Map(dest => dest.RetryCount, src => src.RetryCount)
            .Map(dest => dest.ScheduledFor, src => src.ScheduledFor)
            .Map(dest => dest.SentAt, src => src.SentAt)
            .Map(dest => dest.Recipient, src => src.Recipient);
    }
}
