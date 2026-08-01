using System.Text.Json;
using EHRPlatform.Common.Shared.Utilities.Helpers;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace EHRPlatform.Common.Infrastructure.Caching;

/// <summary>
/// Redis-based distributed cache implementation.
/// Provides production-grade caching with connection pooling and JSON serialization.
/// </summary>
public class RedisCacheService : ICacheService
{
    private readonly IConnectionMultiplexer _connectionMultiplexer;
    private readonly ILogger<RedisCacheService> _logger;

    public RedisCacheService(IConnectionMultiplexer connectionMultiplexer, ILogger<RedisCacheService> logger)
    {
        _connectionMultiplexer = connectionMultiplexer ?? throw new ArgumentNullException(nameof(connectionMultiplexer));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class
    {
        try
        {
            var db = _connectionMultiplexer.GetDatabase();
            var value = await db.StringGetAsync(key);

            if (!value.HasValue)
            {
                _logger.LogDebug("Cache miss: {CacheKey}", key);
                return null;
            }

            _logger.LogDebug("Cache hit: {CacheKey}", key);
            return JsonSerializationHelper.Deserialize<T>(value.ToString());
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error retrieving from cache: {CacheKey}", key);
            return null; // Fail gracefully
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default) where T : class
    {
        try
        {
            var db = _connectionMultiplexer.GetDatabase();
            var serialized = JsonSerializationHelper.Serialize(value);
            await db.StringSetAsync(key, serialized, expiration);

            _logger.LogDebug("Cached: {CacheKey} (TTL: {Expiration}ms)", key, expiration?.TotalMilliseconds);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error writing to cache: {CacheKey}", key);
            // Don't throw - cache failures should not break the application
        }
    }

    public async Task<T> GetOrSetAsync<T>(
        string key,
        Func<string, Task<T>> factory,
        TimeSpan? expiration = null,
        CancellationToken cancellationToken = default) where T : class
    {
        // Try to get from cache first
        var cached = await GetAsync<T>(key, cancellationToken);
        if (cached != null)
        {
            return cached;
        }

        // Load from factory
        _logger.LogDebug("Loading from source: {CacheKey}", key);
        var result = await factory(key);

        // Store in cache
        if (result != null)
        {
            await SetAsync(key, result, expiration, cancellationToken);
        }

        return result;
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            var db = _connectionMultiplexer.GetDatabase();
            await db.KeyDeleteAsync(key);

            _logger.LogDebug("Removed from cache: {CacheKey}", key);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error removing from cache: {CacheKey}", key);
        }
    }

    public async Task RemoveByPatternAsync(string pattern, CancellationToken cancellationToken = default)
    {
        try
        {
            var server = _connectionMultiplexer.GetServer(_connectionMultiplexer.GetEndPoints().First());
            var keys = new List<RedisKey>();

            // Use SCAN for non-blocking iteration
            await foreach (var key in server.KeysAsync(pattern: pattern))
            {
                keys.Add(key);
            }

            if (keys.Count == 0)
            {
                _logger.LogDebug("No keys matched pattern: {Pattern}", pattern);
                return;
            }

            var db = _connectionMultiplexer.GetDatabase();
            await db.KeyDeleteAsync(keys.ToArray());

            _logger.LogInformation("Removed {Count} keys matching pattern: {Pattern}", keys.Count, pattern);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error removing keys by pattern: {Pattern}", pattern);
        }
    }

    public async Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            var db = _connectionMultiplexer.GetDatabase();
            return await db.KeyExistsAsync(key);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error checking key existence: {CacheKey}", key);
            return false;
        }
    }

    public async Task ExpireAsync(string key, TimeSpan expiration, CancellationToken cancellationToken = default)
    {
        try
        {
            var db = _connectionMultiplexer.GetDatabase();
            await db.KeyExpireAsync(key, expiration);

            _logger.LogDebug("Updated expiration for: {CacheKey} (TTL: {Expiration}ms)", key, expiration.TotalMilliseconds);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error updating expiration: {CacheKey}", key);
        }
    }

    public async Task<long> GetTimeToLiveAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            var db = _connectionMultiplexer.GetDatabase();
            var ttl = await db.KeyTimeToLiveAsync(key);
            return (long?)ttl?.TotalSeconds ?? -2; // -2 if key doesn't exist, -1 if no expiration
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error getting TTL: {CacheKey}", key);
            return -2;
        }
    }

    public async Task<CacheStatistics> GetStatisticsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var server = _connectionMultiplexer.GetServer(_connectionMultiplexer.GetEndPoints().First());
            
            var stats = new CacheStatistics
            {
                CapturedAt = DateTimeHelper.UtcNow
            };

            // Get key count
            var keys = server.Keys().ToList();
            stats.TotalKeys = keys.Count;

            // Optionally get memory info (simplified)
            try
            {
                var info = await server.InfoAsync();
                var memoryInfo = info?.FirstOrDefault(g => g.Key == "memory")?.FirstOrDefault();
                // Note: Redis INFO response structure is complex, simplified here
            }
            catch
            {
                // Gracefully handle if INFO fails
            }

            return stats;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error getting cache statistics");
            return new CacheStatistics();
        }
    }
}

