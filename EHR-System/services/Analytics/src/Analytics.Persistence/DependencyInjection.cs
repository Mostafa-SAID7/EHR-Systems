namespace EHRPlatform.Services.Analytics.Persistence;

using Microsoft.Extensions.DependencyInjection;
using EHRPlatform.Services.Analytics.Domain.Repositories;
using EHRPlatform.Services.Analytics.Persistence.Repositories;

/// <summary>
/// Dependency injection for Analytics Persistence layer
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds persistence services and repositories
    /// </summary>
    public static IServiceCollection AddPersistenceServices(this IServiceCollection services)
    {
        // Register repository implementations
        services.AddScoped<IDashboardRepository, DashboardRepository>();
        services.AddScoped<IKPIRepository, KPIRepository>();
        services.AddScoped<IMetricRepository, AnalyticsMetricRepository>();

        return services;
    }
}

