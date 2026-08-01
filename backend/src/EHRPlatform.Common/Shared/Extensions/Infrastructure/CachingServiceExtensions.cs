#nullable enable

using Microsoft.Extensions.DependencyInjection;
using Serilog;
using StackExchange.Redis;
using EHRPlatform.Common.Infrastructure.Health;

namespace EHRPlatform.Common.Infrastructure.Caching;

/// <summary>
/// Extension methods for registering Redis caching services.
/// Single responsibility: Manage caching infrastructure registration.
/// </summary>
public static class CachingServiceExtensions
{
    /// <summary>
    /// Add Redis caching with fail-fast behavior.
    /// Connection failures throw at startup to catch configuration errors early.
    /// </summary>
    public static IServiceCollection AddRedisCaching(
        this IServiceCollection services,
        string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException(
                "Redis connection string is required. Set EHRCommon:RedisConnectionString.");

        try
        {
            var connectionMultiplexer = ConnectionMultiplexer.Connect(connectionString);
            services.AddSingleton<IConnectionMultiplexer>(connectionMultiplexer);
            services.AddSingleton<ICacheService, RedisCacheService>();
            services.AddHealthChecks().AddCacheHealthCheck();
            return services;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                $"Failed to connect to Redis at {connectionString}", ex);
        }
    }

    /// <summary>
    /// Add optional Redis caching with graceful degradation.
    /// Logs a warning if connection string is empty or connection fails.
    /// Use this variant when Redis is optional and the service should start without it.
    /// </summary>
    public static IServiceCollection AddOptionalRedisCaching(
        this IServiceCollection services,
        string? connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            Log.Warning("Redis connection string is empty — caching disabled");
            return services;
        }

        try
        {
            var mux = ConnectionMultiplexer.Connect(connectionString);
            services.AddSingleton<IConnectionMultiplexer>(mux);
            services.AddSingleton<ICacheService, RedisCacheService>();
            return services;
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Redis not available ({ConnectionString}) — caching disabled", connectionString);
            return services;
        }
    }
}
