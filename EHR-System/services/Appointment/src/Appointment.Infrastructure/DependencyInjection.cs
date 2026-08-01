using Microsoft.Extensions.DependencyInjection;

namespace EHRPlatform.Services.Appointment.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
    {
        // Register external service integrations here
        // Example: Email providers, SMS gateways, etc.
        
        return services;
    }
}
