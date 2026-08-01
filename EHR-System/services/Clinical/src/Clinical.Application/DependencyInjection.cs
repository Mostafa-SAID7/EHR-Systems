namespace EHRPlatform.Services.Clinical.Application;

using Microsoft.Extensions.DependencyInjection;
using MediatR;
using System.Reflection;

/// <summary>
/// Dependency injection configuration for Clinical Application layer
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds Clinical Application services to the dependency injection container
    /// </summary>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // MediatR for CQRS
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
        
        return services;
    }
}
