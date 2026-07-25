# Caching Strategies & Redis Integration

Implementation patterns for in-memory and Redis distributed caching in backend microservices.

---

## 1. Cache-Aside Pattern with Redis

```csharp
public class RedisCacheService : ICacheService
{
    private readonly IDatabase _redisDb;

    public RedisCacheService(IConnectionMultiplexer redis)
    {
        _redisDb = redis.GetDatabase();
    }

    public async Task<T?> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan expiration)
    {
        var cached = await _redisDb.StringGetAsync(key);
        if (cached.HasValue)
        {
            return JsonSerializer.Deserialize<T>(cached!);
        }

        var newValue = await factory();
        if (newValue != null)
        {
            await _redisDb.StringSetAsync(key, JsonSerializer.Serialize(newValue), expiration);
        }

        return newValue;
    }
}
```

---

## 2. Handling Cache Stampede & Thundering Herd

Use `SemaphoreSlim` to synchronize concurrent cache misses so only 1 request hits the database while others await the cached result.
