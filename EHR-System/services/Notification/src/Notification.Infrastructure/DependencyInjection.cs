namespace EHRPlatform.Services.Notification.Infrastructure;

using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Dependency injection for Notification Infrastructure layer
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds infrastructure services
    /// </summary>
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
    {
        return services;
    }
}

namespace EHRPlatform.Services.Notification.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
    {
        // Register infrastructure services here (Kafka, external APIs, etc.)
        return services;
    }
}
