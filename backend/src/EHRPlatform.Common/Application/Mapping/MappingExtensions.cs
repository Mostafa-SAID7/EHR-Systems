#nullable enable

namespace EHRPlatform.Common.Application.Mapping;

using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using Mapster;

/// <summary>
/// Extension methods for mapping configuration.
/// </summary>
public static class MappingExtensions
{
    /// <summary>
    /// Register all Mapster IRegister implementations from specified assembly.
    /// Auto-discovers and registers all mapping profiles.
    /// </summary>
    public static IServiceCollection AddMapsterProfiles(
        this IServiceCollection services,
        params Assembly[] assemblies)
    {
        var assemblyList = assemblies.Length > 0 
            ? assemblies 
            : new[] { Assembly.GetCallingAssembly() };

        // Auto-register all IRegister implementations
        foreach (var assembly in assemblyList)
        {
            var registerTypes = assembly.GetTypes()
                .Where(t => typeof(IRegister).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
                .ToList();

            foreach (var registerType in registerTypes)
            {
                var instance = Activator.CreateInstance(registerType) as IRegister;
                instance?.Register(TypeAdapterConfig.GlobalSettings);
            }
        }

        // Compile all mappings for better performance
        TypeAdapterConfig.GlobalSettings.Compile();

        return services;
    }

    /// <summary>
    /// Register all mapper classes from specified assembly.
    /// Discovers classes ending with "Mapper" and implements IMappingService.
    /// </summary>
    public static IServiceCollection AddServiceMappers(
        this IServiceCollection services,
        params Assembly[] assemblies)
    {
        var assemblyList = assemblies.Length > 0 
            ? assemblies 
            : new[] { Assembly.GetCallingAssembly() };

        foreach (var assembly in assemblyList)
        {
            var mapperTypes = assembly.GetTypes()
                .Where(t => t.Name.EndsWith("Mapper") 
                    && !t.IsInterface 
                    && !t.IsAbstract
                    && !t.IsGenericTypeDefinition)
                .ToList();

            foreach (var mapperType in mapperTypes)
            {
                // Register mapper as self for direct injection
                services.AddScoped(mapperType);

                // Also register implemented interfaces
                var interfaces = mapperType.GetInterfaces()
                    .Where(i => i != typeof(IMappingService))
                    .ToList();

                foreach (var @interface in interfaces)
                {
                    services.AddScoped(@interface, sp => sp.GetRequiredService(mapperType));
                }
            }
        }

        return services;
    }

    /// <summary>
    /// Register specific mapper class.
    /// </summary>
    public static IServiceCollection AddMapper<TMapper>(this IServiceCollection services)
        where TMapper : class
    {
        services.AddScoped<TMapper>();
        return services;
    }

    /// <summary>
    /// Register specific Mapster profile (IRegister).
    /// </summary>
    public static IServiceCollection AddMappingProfile<TProfile>(this IServiceCollection services)
        where TProfile : class, IRegister, new()
    {
        var profile = new TProfile();
        profile.Register(TypeAdapterConfig.GlobalSettings);
        TypeAdapterConfig.GlobalSettings.Compile();
        return services;
    }
}

