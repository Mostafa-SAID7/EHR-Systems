namespace EHRPlatform.Services.Clinical.API;

using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Dependency injection configuration for Clinical API layer
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds Clinical API services to the dependency injection container
    /// </summary>
    public static IServiceCollection AddAPIServices(this IServiceCollection services)
    {
        // API layer services can be added here if needed
        return services;
    }
}
