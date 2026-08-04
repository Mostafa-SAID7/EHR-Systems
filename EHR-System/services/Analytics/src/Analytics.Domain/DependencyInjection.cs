namespace EHRPlatform.Services.Analytics.Domain;

using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Dependency injection for Analytics Domain layer
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds domain services
    /// </summary>
    public static IServiceCollection AddDomainServices(this IServiceCollection services)
    {
        // Domain services are typically injected through other layers
        // Repository implementations are registered in Persistence/Infrastructure layers
        return services;
    }
}
