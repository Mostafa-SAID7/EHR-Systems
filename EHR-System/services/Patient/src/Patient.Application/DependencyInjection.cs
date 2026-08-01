using Microsoft.Extensions.DependencyInjection;
using MediatR;

namespace EHRPlatform.Services.Patient.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Register MediatR
        services.AddMediatR(typeof(DependencyInjection));
        
        // Register AutoMapper
        services.AddAutoMapper(typeof(DependencyInjection));
        
        return services;
    }
}
