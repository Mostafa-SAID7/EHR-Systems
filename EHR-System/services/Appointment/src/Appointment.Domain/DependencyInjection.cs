using Microsoft.Extensions.DependencyInjection;

namespace EHRPlatform.Services.Appointment.Domain;

public static class DependencyInjection
{
    public static IServiceCollection AddDomainServices(this IServiceCollection services)
    {
        // Register domain services here
        return services;
    }
}
