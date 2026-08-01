namespace EHRPlatform.Services.Clinical.Contracts;

using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Dependency injection configuration for Clinical Contracts layer
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds Clinical Contracts services to the dependency injection container
    /// </summary>
    public static IServiceCollection AddContractServices(this IServiceCollection services)
    {
        // Contracts layer typically has no services, only DTOs and events
        return services;
    }
}
