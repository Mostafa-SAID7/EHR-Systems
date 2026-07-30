using EHRPlatform.Common.Infrastructure.Caching;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace EHRPlatform.Common.Shared.Extensions.Data;

/// <summary>
/// DI extensions for Redis distributed caching.
///
/// Typical microservice Program.cs usage:
/// <code>
/// builder.Services
///     .AddRedisCaching(redisConnectionString);
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
}
