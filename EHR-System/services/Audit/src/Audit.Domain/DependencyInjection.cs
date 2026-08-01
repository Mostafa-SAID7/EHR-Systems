namespace EHRPlatform.Services.Audit.Domain;

using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Dependency injection for Audit Domain layer
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

namespace EHRPlatform.Services.Audit.Domain;

public static class DependencyInjection
{
    public static IServiceCollection AddDomainServices(this IServiceCollection services)
    {
        // Register domain services here
        return services;
    }
}
