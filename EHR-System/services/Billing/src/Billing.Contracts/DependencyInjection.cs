namespace EHRPlatform.Services.Billing.Contracts;

using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Dependency injection configuration for Billing Contracts layer
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds Billing Contracts services
    /// </summary>
    public static IServiceCollection AddContractServices(this IServiceCollection services)
    {
        return services;
    }
}
