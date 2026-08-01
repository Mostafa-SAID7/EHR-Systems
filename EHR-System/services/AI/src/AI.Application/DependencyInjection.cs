using Microsoft.Extensions.DependencyInjection;

namespace EHRPlatform.Services.AI.Application;

/// <summary>
/// Dependency Injection configuration for AI Service Application Layer
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registers AI service application services
    /// </summary>
    public static IServiceCollection AddAIApplicationServices(
        this IServiceCollection services)
    {
        // MediatR registration would go here
        // AutoMapper registration would go here
        
        return services;
    }
}
