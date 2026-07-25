# Caching Strategies

## Cache-Aside Pattern

```csharp
public class UserService
{
    private readonly IRepository<User> _repository;
    private readonly IDistributedCache _cache;
    
    public async Task<User> GetUserAsync(int id)
    {
        // 1. Try cache first
        var cached = await _cache.GetStringAsync($"user:{id}");
        if (cached != null)
            return JsonSerializer.Deserialize<User>(cached);
        
        // 2. Cache miss - query database
        var user = await _repository.GetByIdAsync(id);
        
        // 3. Store in cache
        await _cache.SetStringAsync(
            $"user:{id}",
            JsonSerializer.Serialize(user),
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1)
            }
        );
        
        return user;
    }
}
```

---

## Write-Through Cache

```csharp
public async Task<User> UpdateUserAsync(int id, UpdateUserRequest request)
{
    // 1. Update database
    var user = await _repository.UpdateAsync(id, request);
    
    // 2. Update cache
    await _cache.SetStringAsync(
        $"user:{id}",
        JsonSerializer.Serialize(user),
        new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1) }
    );
    
    return user;
}
```

---

## Cache Invalidation

```csharp
// Invalidate on create/update/delete
public async Task DeleteUserAsync(int id)
{
    await _repository.DeleteAsync(id);
    
    // Invalidate cache
    await _cache.RemoveAsync($"user:{id}");
    
    // Invalidate list cache (users:page:1, users:page:2, etc.)
    await _cache.RemoveAsync("users:*"); // Pattern invalidation
}
```

---

## Redis Setup

```csharp
// Program.cs
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
});

// Usage
var cache = serviceProvider.GetRequiredService<IDistributedCache>();
await cache.SetStringAsync("key", "value");
var value = await cache.GetStringAsync("key");
```

---

## TTL (Time To Live)

```csharp
// Short TTL - Volatile data
var shortCache = new DistributedCacheEntryOptions
{
    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
};

// Long TTL - Static data
var longCache = new DistributedCacheEntryOptions
{
    AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(7)
};

// Sliding expiration - Active users
var slidingCache = new DistributedCacheEntryOptions
{
    SlidingExpiration = TimeSpan.FromHours(1) // Extends on each access
};
```

---

## Interview Q&A

**Q: When to cache?**

A:
- Read-heavy data (user profiles, settings)
- Expensive queries (aggregations, reports)
- NOT frequently changing data
- NOT business-critical data that must be current

**Q: Cache invalidation problems?**

A:
- Stale data: Cache not updated after database change
- Cascading invalidation: Invalidating all related caches
- Memory bloat: Too much cached data
