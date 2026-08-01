namespace EHRPlatform.Services.Notification.Domain;

using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Dependency injection for Notification Domain layer
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds domain services
    /// </summary>
    public static IServiceCollection AddDomainServices(this IServiceCollection services)
    {
        return services;
    }
}

namespace EHRPlatform.Services.Notification.Domain;

public static class DependencyInjection
{
    public static IServiceCollection AddDomainServices(this IServiceCollection services)
    {
        // Register domain services here
        return services;
    }
}
