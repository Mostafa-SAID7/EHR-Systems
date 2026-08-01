using Microsoft.Extensions.DependencyInjection;

namespace EHRPlatform.Services.Patient.Persistence;

public static class DependencyInjection
{
    public static IServiceCollection AddPersistenceServices(this IServiceCollection services)
    {
        // Add DbContext and repositories here
        // Example: services.AddDbContext<PatientContext>();
        
        return services;
    }
}
