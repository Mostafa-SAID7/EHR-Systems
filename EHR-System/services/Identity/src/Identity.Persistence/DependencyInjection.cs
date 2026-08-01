using Microsoft.Extensions.DependencyInjection;

namespace EHRPlatform.Services.Identity.Persistence;

public static class DependencyInjection
{
    public static IServiceCollection AddPersistenceServices(this IServiceCollection services)
    {
        // Add DbContext and repositories here
        // Example: services.AddDbContext<IdentityContext>();
        
        return services;
    }
}
