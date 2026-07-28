#nullable enable

using System;
using System.Threading.Tasks;
using StackExchange.Redis;
using Testcontainers.Redis;

namespace EHRPlatform.Tests.Common.Fixtures;

/// <summary>
/// Redis Testcontainer fixture for cache testing.
/// Manages lifecycle: container creation, connection, cleanup.
/// </summary>
public class CacheFixture : IAsyncLifetime
{
    private readonly RedisContainer _container;
    private IConnectionMultiplexer? _connection;

    public IConnectionMultiplexer Connection
    {
        get => _connection ?? throw new InvalidOperationException("Cache not initialized");
        private set => _connection = value;
    }

    public IDatabase Db => Connection.GetDatabase();

    public CacheFixture()
    {
        _container = new RedisBuilder()
            .WithImage("redis:7-alpine")
            .WithCleanUp(true)
            .Build();
    }

    /// <summary>
    /// Start Redis container and establish connection.
    /// </summary>
    public async Task InitializeAsync()
    {
        await _container.StartAsync();
        var connectionString = _container.GetConnectionString();

        Connection = await ConnectionMultiplexer.ConnectAsync(connectionString);
    }

    /// <summary>
    /// Close connection and stop container.
    /// </summary>
    public async Task DisposeAsync()
    {
        if (_connection != null)
        {
            await _connection.CloseAsync();
            _connection.Dispose();
        }

        if (_container != null)
        {
            await _container.StopAsync();
            await _container.DisposeAsync();
        }
    }

    /// <summary>
    /// Clear all cache entries.
    /// </summary>
    public async Task FlushAllAsync()
    {
        var endpoints = Connection.GetServer(Connection.GetEndPoints().First());
        await endpoints.FlushAllAsync();
    }

    /// <summary>
    /// Get cache key with optional prefix.
    /// </summary>
    public async Task<string?> GetAsync(string key, string prefix = "")
    {
        var fullKey = string.IsNullOrEmpty(prefix) ? key : $"{prefix}:{key}";
        var value = await Db.StringGetAsync(fullKey);
        return value.IsNullOrEmpty ? null : value.ToString();
    }

    /// <summary>
    /// Set cache value with optional expiration.
    /// </summary>
    public async Task SetAsync(string key, string value, TimeSpan? expiration = null, string prefix = "")
    {
        var fullKey = string.IsNullOrEmpty(prefix) ? key : $"{prefix}:{key}";
        await Db.StringSetAsync(fullKey, value, expiration);
    }

    /// <summary>
    /// Remove cache key.
    /// </summary>
    public async Task RemoveAsync(string key, string prefix = "")
    {
        var fullKey = string.IsNullOrEmpty(prefix) ? key : $"{prefix}:{key}";
        await Db.KeyDeleteAsync(fullKey);
    }

    /// <summary>
    /// Check if key exists.
    /// </summary>
    public async Task<bool> KeyExistsAsync(string key, string prefix = "")
    {
        var fullKey = string.IsNullOrEmpty(prefix) ? key : $"{prefix}:{key}";
        return await Db.KeyExistsAsync(fullKey);
    }

    /// <summary>
    /// Get all keys matching pattern.
    /// </summary>
    public async Task<string[]> GetKeysByPatternAsync(string pattern)
    {
        var endpoints = Connection.GetServer(Connection.GetEndPoints().First());
        var keys = await endpoints.KeysAsync(pattern: pattern);
        return keys.Select(k => k.ToString()).ToArray();
    }
}
