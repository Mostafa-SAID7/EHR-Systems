namespace EHRPlatform.Services.Billing.API;

using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Dependency injection configuration for Billing API layer
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds Billing API services
    /// </summary>
    public static IServiceCollection AddAPIServices(this IServiceCollection services)
    {
        return services;
    }
}
