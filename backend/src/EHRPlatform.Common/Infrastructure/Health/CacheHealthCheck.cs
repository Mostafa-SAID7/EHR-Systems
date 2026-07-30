using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using EHRPlatform.Common.Infrastructure.Caching;

namespace EHRPlatform.Common.Infrastructure.Health;

/// <summary>
/// Health check for Redis cache connectivity and performance.
/// Verifies that Redis is accessible and operational.
/// </summary>
public class CacheHealthCheck : IHealthCheck
{
    private readonly ICacheService _cacheService;
    private readonly ILogger<CacheHealthCheck> _logger;

    public CacheHealthCheck(ICacheService cacheService, ILogger<CacheHealthCheck> logger)
    {
        _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            // Test basic cache operations
            var testKey = $"health-check-{DateTime.UtcNow.Ticks}";
            var testValue = "health-check-test";

            // Test SET
            await _cacheService.SetAsync(testKey, testValue, TimeSpan.FromSeconds(10), cancellationToken);

            // Test GET
            var retrieved = await _cacheService.GetAsync<string>(testKey, cancellationToken);

            // Test DELETE
            await _cacheService.RemoveAsync(testKey, cancellationToken);

            // Get statistics
            var stats = await _cacheService.GetStatisticsAsync(cancellationToken);

            if (retrieved != testValue)
            {
                _logger.LogWarning("Cache health check: GET/SET verification failed");
                return HealthCheckResult.Unhealthy("Cache get/set verification failed");
            }

            var data = new Dictionary<string, object>
            {
                { "status", "cache-operational" },
                { "total_keys", stats.TotalKeys },
                { "used_memory_bytes", stats.UsedMemoryBytes },
                { "max_memory_bytes", stats.MaxMemoryBytes },
                { "captured_at", stats.CapturedAt }
            };

            _logger.LogInformation("Cache health check passed. Keys: {KeyCount}, Memory: {UsedMemory}MB", 
                stats.TotalKeys, 
                stats.UsedMemoryBytes / (1024 * 1024));

            return HealthCheckResult.Healthy("Cache is operational", data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Cache health check failed");
            return HealthCheckResult.Unhealthy("Cache health check failed", ex);
        }
    }
}

/// <summary>
/// Extension methods for registering cache health checks.
/// </summary>
public static class CacheHealthCheckExtensions
{
    /// <summary>
    /// Add Redis cache health check to health check service.
    /// </summary>
    public static IHealthChecksBuilder AddCacheHealthCheck(
        this IHealthChecksBuilder builder,
        string? name = null,
        HealthStatus? failureStatus = null,
        IEnumerable<string>? tags = null,
        TimeSpan? timeout = null)
    {
        return builder.AddCheck<CacheHealthCheck>(
            name ?? "redis-cache",
            failureStatus ?? HealthStatus.Unhealthy,
            tags ?? new[] { "cache", "redis" },
            timeout ?? TimeSpan.FromSeconds(5));
    }
}

