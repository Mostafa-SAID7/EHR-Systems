namespace EHRPlatform.Services.Notification.API;

using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Dependency injection for Notification API layer
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds API services
    /// </summary>
    public static IServiceCollection AddAPIServices(this IServiceCollection services)
    {
        return services;
    }
}

namespace EHRPlatform.Services.Notification.API;

public static class DependencyInjection
{
    public static IServiceCollection AddAPIServices(this IServiceCollection services)
    {
        // Register API-level services here
        return services;
    }
}
