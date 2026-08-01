namespace EHRPlatform.Services.Audit.Persistence;

using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Dependency injection for Audit Persistence layer
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds persistence services
    /// </summary>
    public static IServiceCollection AddPersistenceServices(this IServiceCollection services)
    {
        return services;
    }
}
using Microsoft.Extensions.DependencyInjection;

namespace EHRPlatform.Services.Audit.Persistence;

public static class DependencyInjection
{
    public static IServiceCollection AddPersistenceServices(
        this IServiceCollection services,
        string connectionString)
    {
        // Register DbContext
        services.AddDbContext<AuditDbContext>(options =>
            options.UseSqlServer(connectionString));

        // Register repositories here
        
        return services;
    }
}
