using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using EHRPlatform.Services.Notification.Application.Features.Notifications.Commands;

namespace EHRPlatform.Services.Notification.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // MediatR - auto-registers all handlers from assembly
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

        // FluentValidation
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        // AutoMapper
        services.AddAutoMapper(Assembly.GetExecutingAssembly());

        // Explicitly register SetNotificationPreferenceCommandHandler (ensures availability)
        services.AddScoped<IRequestHandler<SetNotificationPreferenceCommand, NotificationResult>, SetNotificationPreferenceCommandHandler>();

        return services;
    }
}
