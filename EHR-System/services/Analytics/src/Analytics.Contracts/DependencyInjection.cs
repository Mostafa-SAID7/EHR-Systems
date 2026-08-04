namespace EHRPlatform.Services.Analytics.Contracts;

using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Dependency injection for Analytics Contracts layer
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds contract services
    /// </summary>
    public static IServiceCollection AddContractServices(this IServiceCollection services)
    {
        // Contract-level service registrations
        return services;
    }
}
