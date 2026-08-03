namespace Identity.Infrastructure;

using Identity.Domain.Interfaces;
using Identity.Infrastructure.Jwt;
using Identity.Infrastructure.PasswordPolicy;
using Identity.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods for dependency injection of infrastructure services
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds infrastructure services to the dependency injection container
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configuration">The application configuration</param>
    /// <returns>The service collection</returns>
    public static IServiceCollection AddIdentityInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // JWT Configuration - now local to Identity service
        var jwtSettings = new JwtSettings();
        configuration.GetSection("Jwt").Bind(jwtSettings);
        services.AddSingleton(jwtSettings);
        services.AddScoped<IJwtTokenProvider, JwtTokenProvider>();

        // Password hashing
        services.AddScoped<IPasswordHasher, PasswordHashingService>();

        // Password policy validation
        services.AddScoped<IPasswordPolicy, PasswordPolicy.PasswordPolicy>();

        return services;
    }
}
