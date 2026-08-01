using Microsoft.Extensions.DependencyInjection;

namespace EHRPlatform.Services.Appointment.Contracts;

public static class DependencyInjection
{
    public static IServiceCollection AddContractServices(this IServiceCollection services)
    {
        // Contract-level service registrations
        return services;
    }
}
