namespace EHRPlatform.Services.Billing.Persistence;

using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Dependency injection configuration for Billing Persistence layer
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds Billing Persistence services
    /// </summary>
    public static IServiceCollection AddPersistenceServices(this IServiceCollection services)
    {
        return services;
    }
}
