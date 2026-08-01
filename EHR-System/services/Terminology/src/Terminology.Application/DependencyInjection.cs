using Microsoft.Extensions.DependencyInjection;

namespace EHRPlatform.Services.Terminology.Application;

/// <summary>
/// Dependency Injection configuration for Terminology Service Application Layer
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registers Terminology service application services
    /// </summary>
    public static IServiceCollection AddTerminologyApplicationServices(
        this IServiceCollection services)
    {
        // MediatR registration would go here
        // AutoMapper registration would go here
        
        return services;
    }
}
