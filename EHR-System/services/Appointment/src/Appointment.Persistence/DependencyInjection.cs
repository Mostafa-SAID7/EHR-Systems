using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EHRPlatform.Services.Appointment.Persistence;

public static class DependencyInjection
{
    public static IServiceCollection AddPersistenceServices(
        this IServiceCollection services,
        string connectionString)
    {
        // Register DbContext
        services.AddDbContext<AppointmentDbContext>(options =>
            options.UseSqlServer(connectionString));

        // Register repositories here
        
        return services;
    }
}
