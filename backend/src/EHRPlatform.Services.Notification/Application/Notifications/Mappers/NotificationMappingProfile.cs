using Mapster;
using EHRPlatform.Services.Notification.Domain.Entities;
using EHRPlatform.Services.Notification.Application.Notifications.Responses;

namespace EHRPlatform.Services.Notification.Application.Notifications.Mappers;

/// <summary>
/// Mapster registration profile for Notification entity mappings.
/// </summary>
public class NotificationMappingProfile : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Notification, NotificationResponse>();
    }
}
