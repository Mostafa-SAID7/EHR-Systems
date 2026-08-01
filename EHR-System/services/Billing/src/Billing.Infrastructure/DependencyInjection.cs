namespace EHRPlatform.Services.Billing.Infrastructure;

using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Dependency injection configuration for Billing Infrastructure layer
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds Billing Infrastructure services
    /// </summary>
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
    {
        return services;
    }
}
