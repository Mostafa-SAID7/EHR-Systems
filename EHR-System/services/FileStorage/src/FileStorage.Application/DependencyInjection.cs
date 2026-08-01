using Microsoft.Extensions.DependencyInjection;

namespace EHRPlatform.Services.FileStorage.Application;

/// <summary>
/// Dependency Injection configuration for FileStorage Service Application Layer
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registers FileStorage service application services
    /// </summary>
    public static IServiceCollection AddFileStorageApplicationServices(
        this IServiceCollection services)
    {
        // MediatR registration would go here
        // AutoMapper registration would go here
        
        return services;
    }
}
