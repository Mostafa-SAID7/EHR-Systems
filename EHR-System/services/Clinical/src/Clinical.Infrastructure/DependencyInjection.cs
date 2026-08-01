namespace EHRPlatform.Services.Clinical.Infrastructure;

using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Dependency injection configuration for Clinical Infrastructure layer
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds Clinical Infrastructure services to the dependency injection container
    /// </summary>
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
    {
        // Infrastructure services (consumers, outbox processor, etc.) can be registered here
        return services;
    }
}
