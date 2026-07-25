# Performance Optimization Tips

## N+1 Query Problem

```csharp
// ❌ BAD - N+1 queries
var users = context.Users.ToList(); // Query 1
foreach (var user in users)
{
    var orders = user.Orders.ToList(); // Query 2..N
}

// ✅ GOOD - Single query with Include
var users = await context.Users
    .Include(u => u.Orders)
    .ToListAsync(); // 1 query
```

---

## Async All The Way

```csharp
// ❌ BAD - Blocks threads
public string GetUser(int id)
{
    var result = _userService.GetUserAsync(id).Result; // Deadlock!
    return result.Name;
}

// ✅ GOOD - Async/await
public async Task<string> GetUserAsync(int id)
{
    var result = await _userService.GetUserAsync(id);
    return result.Name;
}
```

---

## Pagination

```csharp
// ❌ BAD - Load all
var all = await context.Users.ToListAsync();

// ✅ GOOD - Paginate
var page = await context.Users
    .Skip((pageNumber - 1) * pageSize)
    .Take(pageSize)
    .ToListAsync();
```

---

## Caching

```csharp
// ❌ BAD - Query every time
var settings = await context.Settings.ToListAsync();

// ✅ GOOD - Cache static data
var settings = await _cache.GetOrCreateAsync("settings", async entry =>
{
    entry.SlidingExpiration = TimeSpan.FromHours(1);
    return await context.Settings.ToListAsync();
});
```

---

## Batch Operations

```csharp
// ❌ BAD - Individual saves
foreach (var user in users)
{
    await context.Users.AddAsync(user);
    await context.SaveChangesAsync(); // N saves
}

// ✅ GOOD - Batch save
await context.Users.AddRangeAsync(users);
await context.SaveChangesAsync(); // 1 save
```

---

## Interview Q&A

**Q: How to profile slow code?**

A:
- Entity Framework Profiler
- Stopwatch for timing
- Query execution plans
- Application Insights
