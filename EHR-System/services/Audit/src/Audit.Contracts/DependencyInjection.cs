namespace EHRPlatform.Services.Audit.Contracts;

using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Dependency injection for Audit Contracts layer
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds contract services
    /// </summary>
    public static IServiceCollection AddContractServices(this IServiceCollection services)
    {
        return services;
    }
}

namespace EHRPlatform.Services.Audit.Contracts;

public static class DependencyInjection
{
    public static IServiceCollection AddContractServices(this IServiceCollection services)
    {
        // Contract-level service registrations
        return services;
    }
}
