using EHRPlatform.Common.Infrastructure.Caching.Handlers;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace EHRPlatform.Common.Infrastructure.Caching;

/// <summary>
/// DI extensions for Redis distributed caching and cache invalidation.
///
/// Typical microservice Program.cs usage:
/// <code>
/// builder.Services
///     .AddRedisCaching(redisConnectionString)
///     .AddCacheInvalidationHandlers();
/// </code>
/// </summary>
public static class CachingExtensions
{
    /// <summary>
    /// Register Redis distributed caching.
    /// </summary>
    public static IServiceCollection AddRedisCaching(
        this IServiceCollection services,
        string? connectionString)
    {
        if (string.IsNullOrEmpty(connectionString))
            throw new ArgumentException("Redis connection string is required.", nameof(connectionString));

        services.AddSingleton<IConnectionMultiplexer>(_ =>
        {
            var opts = ConfigurationOptions.Parse(connectionString);
            opts.AbortOnConnectFail = false;
            opts.ConnectTimeout     = 5_000;
            opts.SyncTimeout        = 5_000;
            return ConnectionMultiplexer.Connect(opts);
        });

        services.AddSingleton<ICacheService, RedisCacheService>();

        return services;
    }

    /// <summary>
    /// Register domain-specific cache invalidation handlers.
    /// </summary>
    public static IServiceCollection AddCacheInvalidationHandlers(this IServiceCollection services)
    {
        services.AddScoped<PatientCacheInvalidationHandler>();
        services.AddScoped<AppointmentCacheInvalidationHandler>();
        services.AddScoped<ClinicalCacheInvalidationHandler>();
        services.AddScoped<UserCacheInvalidationHandler>();
        services.AddScoped<ReferenceDataCacheInvalidationHandler>();
        services.AddScoped<CacheInvalidationOrchestrator>();

        return services;
    }
}


