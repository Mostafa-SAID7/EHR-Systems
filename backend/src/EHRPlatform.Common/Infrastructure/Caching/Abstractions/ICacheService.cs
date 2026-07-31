namespace EHRPlatform.Common.Infrastructure.Caching;

using EHRPlatform.Common.Shared.Utilities.Helpers;

/// <summary>
/// Distributed cache service abstraction for Redis.
/// Provides high-level cache operations with automatic serialization.
/// </summary>
public interface ICacheService
{
    /// <summary>
    /// Get a value from cache.
    /// </summary>
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Set a value in cache with optional TTL.
    /// </summary>
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Get a value from cache or load it using the provided factory function.
    /// Atomically prevents thundering herd problem.
    /// </summary>
    Task<T> GetOrSetAsync<T>(
        string key,
        Func<string, Task<T>> factory,
        TimeSpan? expiration = null,
        CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Remove a single key or pattern from cache.
    /// </summary>
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Remove multiple keys matching a pattern (e.g., "patient:*").
    /// </summary>
    Task RemoveByPatternAsync(string pattern, CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if a key exists in cache.
    /// </summary>
    Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Extend or set a new TTL for an existing key.
    /// </summary>
    Task ExpireAsync(string key, TimeSpan expiration, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get remaining TTL for a key in seconds (-1 if no expiration, -2 if key doesn't exist).
    /// </summary>
    Task<long> GetTimeToLiveAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get cache statistics for monitoring and observability.
    /// </summary>
    Task<CacheStatistics> GetStatisticsAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Cache statistics for monitoring.
/// </summary>
public class CacheStatistics
{
    public long TotalKeys { get; set; }
    public long UsedMemoryBytes { get; set; }
    public long MaxMemoryBytes { get; set; }
    public double EvictionRate { get; set; }
    public DateTime CapturedAt { get; set; } = DateTimeHelper.UtcNow;
}

