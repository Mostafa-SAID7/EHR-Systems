using Microsoft.Extensions.DependencyInjection;

namespace EHRPlatform.Services.BFF.Application;

/// <summary>
/// Dependency Injection configuration for BFF Application Layer
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registers BFF application services
    /// </summary>
    public static IServiceCollection AddBFFApplicationServices(
        this IServiceCollection services)
    {
        // MediatR registration for aggregation queries
        // AutoMapper registration for response composition
        
        return services;
    }
}
