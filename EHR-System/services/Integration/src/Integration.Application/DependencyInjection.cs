using Microsoft.Extensions.DependencyInjection;

namespace EHRPlatform.Services.Integration.Application;

/// <summary>
/// Dependency Injection configuration for Integration Service Application Layer
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registers Integration service application services
    /// </summary>
    public static IServiceCollection AddIntegrationApplicationServices(
        this IServiceCollection services)
    {
        // MediatR registration would go here
        // AutoMapper registration would go here
        
        return services;
    }
}
