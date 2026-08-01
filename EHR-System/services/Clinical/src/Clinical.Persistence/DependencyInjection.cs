namespace EHRPlatform.Services.Clinical.Persistence;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// Dependency injection configuration for Clinical Persistence layer
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds Clinical Persistence services to the dependency injection container
    /// </summary>
    public static IServiceCollection AddPersistenceServices(this IServiceCollection services, string? connectionString = null)
    {
        // Database context and repositories can be registered here
        // Example:
        // services.AddDbContext<ClinicalContext>(options =>
        //     options.UseSqlServer(connectionString));
        
        return services;
    }
}
