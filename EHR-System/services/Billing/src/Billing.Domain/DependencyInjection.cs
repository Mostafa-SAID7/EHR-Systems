namespace EHRPlatform.Services.Billing.Domain;

using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Dependency injection configuration for Billing Domain layer
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds Billing Domain services
    /// </summary>
    public static IServiceCollection AddDomainServices(this IServiceCollection services)
    {
        return services;
    }
}
