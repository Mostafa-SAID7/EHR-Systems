namespace EHRPlatform.Services.Billing.Application;

using Microsoft.Extensions.DependencyInjection;
using MediatR;
using System.Reflection;

/// <summary>
/// Dependency injection configuration for Billing Application layer
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds Billing Application services
    /// </summary>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
        return services;
    }
}
