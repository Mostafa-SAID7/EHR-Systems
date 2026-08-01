namespace EHRPlatform.Services.Clinical.Domain;

using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Dependency injection configuration for Clinical Domain layer
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds Clinical Domain services to the dependency injection container
    /// </summary>
    public static IServiceCollection AddDomainServices(this IServiceCollection services)
    {
        // Domain layer services/specifications can be registered here if needed
        return services;
    }
}
