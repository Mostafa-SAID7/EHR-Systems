# Analytics Service - Issues & Solutions

## Common Issues

### Issue #1: Connection String Not Found

**Error**: 
```
InvalidOperationException: No connection string named 'DefaultConnection' found.
```

**Root Cause**: 
- appsettings.json missing ConnectionStrings section
- Environment variables not set
- Running on Replit with no PGHOST variable

**Solution**:

**For Local Development**:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=ehr_analytics;Username=postgres;Password=password;SSL Mode=Disable",
    "Redis": "localhost:6379"
  }
}
```

**For Replit** (environment variables are auto-set by Replit):
```csharp
// Use the BuildConnectionString helper that checks env vars
var connectionString = BuildConnectionString(builder.Configuration);
```

**Verification**:
```bash
# Check PostgreSQL connection
psql -U postgres -d ehr_analytics -c "SELECT 1"

# Check Redis connection
redis-cli PING
```

---

### Issue #2: Cache Not Working (Missing RedisService)

**Symptom**: 
- Queries always slow (no cache benefit)
- Large numbers of database queries
- Red indicators on Redis logs

**Root Cause**:
- IDistributedCache not registered in DI
- Redis registration wrapped in try/catch but service doesn't handle null cache

**Solution**:

```csharp
// CORRECT: Service handles null cache gracefully
public class AnalyticsService
{
    private readonly IDistributedCache? _cache;  // Nullable!
    
    public async Task<Report> GetReportAsync(Guid id)
    {
        // Try cache first if available
        if (_cache != null)
        {
            var cached = await _cache.GetStringAsync($"report:{id}");
            if (!string.IsNullOrEmpty(cached))
                return JsonSerializer.Deserialize<Report>(cached);
        }
        
        // Fall back to database
        var report = await _dbContext.AnalyticsReports.FindAsync(id);
        
        // Cache if available
        if (_cache != null && report != null)
        {
            await _cache.SetStringAsync($"report:{id}", 
                JsonSerializer.Serialize(report),
                new DistributedCacheEntryOptions { 
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1) 
                });
        }
        
        return report;
    }
}
```

---

### Issue #3: Elasticsearch Not Replicating Data

**Symptom**:
- Search returns no results
- Elasticsearch index empty
- OutboxEvents pile up in database

**Root Cause**:
- OutboxEventProcessor not running
- Elasticsearch connection string wrong
- Network connectivity issue

**Solution**:

**Step 1**: Verify Elasticsearch is running
```bash
curl http://localhost:9200
# Should return cluster info

# Check if index exists
curl http://localhost:9200/_cat/indices
```

**Step 2**: Register OutboxEventProcessor
```csharp
// In Program.cs
services.AddHostedService<OutboxEventProcessor>();

// OutboxEventProcessor.cs
public class OutboxEventProcessor : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AnalyticsDbContext>();
            var elasticsearchClient = scope.ServiceProvider.GetRequiredService<ElasticsearchClient>();
            
            // Get unprocessed events
            var unprocessed = await dbContext.OutboxEvents
                .Where(e => e.ProcessedAt == null)
                .Take(100)
                .ToListAsync(stoppingToken);
            
            foreach (var evt in unprocessed)
            {
                try
                {
                    // Parse and index in Elasticsearch
                    var report = JsonSerializer.Deserialize<AnalyticsReport>(evt.Payload);
                    await elasticsearchClient.IndexAsync(report, 
                        i => i.Index("analytics_reports")
                            .Id(report.Id.ToString()),
                        stoppingToken);
                    
                    // Mark as processed
                    evt.ProcessedAt = DateTime.UtcNow;
                    await dbContext.SaveChangesAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    // Log error but don't stop processing others
                    logger.LogError(ex, "Failed to process outbox event {eventId}", evt.Id);
                }
            }
            
            // Wait before next batch
            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        }
    }
}
```

---

### Issue #4: Redis Memory Growing Unbounded

**Symptom**:
- Redis using 500MB, 1GB, 2GB+
- Redis OOM (Out of Memory) errors
- Cache stops working

**Root Cause**:
- TTL not set on cache entries
- Cache entries never evicted
- Too aggressive caching

**Solution**:

```csharp
// CORRECT: Always set TTL
var options = new DistributedCacheEntryOptions
{
    AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1),  // Key!
    SlidingExpiration = TimeSpan.FromMinutes(10)
};

await _cache.SetStringAsync("report:123", data, options);
```

**Monitor Redis memory**:
```bash
# Check memory usage
redis-cli INFO memory

# Set max memory policy
redis-cli CONFIG SET maxmemory 256mb
redis-cli CONFIG SET maxmemory-policy allkeys-lru
```

---

### Issue #5: Cascading Failure (If Redis down, whole service slow)

**Symptom**:
- Single Redis failure causes 100x slowdown
- Database connections overwhelmed
- Service becomes unresponsive

**Root Cause**:
- Cache lookup is synchronous and times out
- Timeout is long (default 5 seconds)
- Failed cache lookups cause cascade

**Solution**:

```csharp
// Configure short timeout for Redis operations
services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = "localhost:6379";
    options.InstanceName = "analytics:";
    options.Configuration += ",connectTimeout=1000";  // 1 second timeout!
});

// In service: Catch timeout quickly
public async Task<Report> GetReportAsync(Guid id)
{
    try
    {
        var cached = await _cache.GetStringAsync($"report:{id}");
        if (!string.IsNullOrEmpty(cached))
            return JsonSerializer.Deserialize<Report>(cached);
    }
    catch (TimeoutException)
    {
        logger.LogWarning("Cache timeout - using database");
        // Fall through to database (fast failure)
    }
    
    return await _dbContext.AnalyticsReports.FindAsync(id);
}
```

---

## Performance Issues

### Issue: Query N+1 Problem

**Problem**: Loop over reports, then query each report's details

```csharp
// ❌ Bad: N+1 queries
var reports = await _dbContext.AnalyticsReports.ToListAsync();
foreach (var report in reports)
{
    var details = JsonSerializer.Deserialize<ReportDetails>(report.Data);
    // This is OK in C# - data is already loaded
}

// But if using navigation properties:
// ❌ Bad: This WOULD be N+1
var users = await _dbContext.Users.ToListAsync();
foreach (var user in users)
{
    var reports = user.Reports.ToList();  // SQL query in loop!
}
```

**Solution**:
```csharp
// ✓ Good: Single query with Include
var users = await _dbContext.Users
    .Include(u => u.Reports)
    .ToListAsync();
```

---

### Issue: Elasticsearch Index Too Large

**Problem**: Index grows to 50GB+, searches slow  

**Solution**:
```csharp
// Delete old data periodically
var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);

await _elasticsearchClient.DeleteByQueryAsync<AnalyticsReport>(d => d
    .Query(q => q.Range(r => r.Field(f => f.CreatedAt)
        .LessThan(thirtyDaysAgo)))
);
```

---

## Deployment Issues

### Issue: Migration Fails on Prod

**Problem**: EF Core migration throws error in production

**Solution**:
```csharp
// Always wrap migrations in try/catch
using (var scope = app.Services.CreateScope())
{
    try
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<AnalyticsDbContext>();
        await dbContext.Database.MigrateAsync();
        logger.LogInformation("Migrations applied successfully");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Migration failed");
        // Check logs to diagnose
        // Don't throw - let app start anyway
    }
}
```

---

### Issue: Connection Pool Exhausted

**Error**: 
```
SqlException: Timeout expired. The timeout period elapsed prior to obtaining a connection from the pool
```

**Root Cause**:
- Service creating too many connections
- Not disposing DbContext properly
- Long-running queries holding connections

**Solution**:
```csharp
// Set appropriate pool size
services.AddDbContext<AnalyticsDbContext>(options =>
    options.UseNpgsql(connectionString, npgsqlOptions =>
        npgsqlOptions.CommandTimeout(30)
            .MaxPoolSize(20)  // Adjust based on load
    )
);

// Always use using for DbContext
using (var dbContext = scope.ServiceProvider.GetRequiredService<AnalyticsDbContext>())
{
    // Use context
}  // Automatically returned to pool
```

---

## Testing Issues

### Issue: Tests Fail Due to "Cascading Deletes"

**Problem**: Test tries to delete report but foreign keys prevent it

**Solution**: Use soft deletes
```csharp
public class AnalyticsReport
{
    public bool IsArchived { get; set; }  // Instead of Delete
}

// Query filter excludes archived
modelBuilder.Entity<AnalyticsReport>()
    .HasQueryFilter(r => !r.IsArchived);
```

---

### Issue: Integration Tests Slow

**Problem**: Tests run against real PostgreSQL, very slow

**Solution**: Use In-Memory SQLite for tests
```csharp
public class AnalyticsTests : IAsyncLifetime
{
    private IServiceProvider _serviceProvider;
    
    public async Task InitializeAsync()
    {
        var builder = new ServiceCollection()
            .AddDbContext<AnalyticsDbContext>(options =>
                options.UseSqlite("Data Source=:memory:")  // In-memory DB
            );
        
        _serviceProvider = builder.BuildServiceProvider();
        
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AnalyticsDbContext>();
        await dbContext.Database.EnsureCreatedAsync();
    }
}
```

---

## Troubleshooting Guide

### Health Check Shows Issues

```
{
  "status": "Degraded",
  "checks": {
    "analytics": {
      "status": "Degraded",
      "data": {
        "PostgreSQL": "✓ Healthy",
        "Redis": "✗ Unavailable",
        "Elasticsearch": "✗ Unavailable"
      }
    }
  }
}
```

**What to do**:
1. Redis unavailable = service is slower but works ✓
2. Elasticsearch unavailable = search disabled but works ✓
3. PostgreSQL unavailable = service fails ✗ (critical!)

**Fix**:
```bash
# Restart Redis
docker restart redis

# Restart Elasticsearch
docker restart elasticsearch

# For PostgreSQL, use backups/failover
```

---

## Prevention Checklist

- [ ] Set TTL on all cache entries
- [ ] Test cache eviction policy under load
- [ ] Monitor database connection pool
- [ ] Set query timeouts (prevent runaway queries)
- [ ] Use soft deletes everywhere
- [ ] Implement health checks for all stores
- [ ] Test graceful degradation manually
- [ ] Monitor Elasticsearch index size
- [ ] Set connection string short timeouts
- [ ] Review migrations before production deploy

---

## Emergency Response

### If PostgreSQL Dies
1. Failover to replica (seconds)
2. Or restore from backup
3. All optional stores can wait

### If Redis Dies
1. Service continues (slower)
2. Restart Redis
3. Cache gradually repopulates

### If Elasticsearch Dies
1. Service continues (search disabled)
2. Restart Elasticsearch
3. Reprocess outbox events to reindex

### If Multiple Stores Down
1. Check health endpoint
2. Use status page to inform users
3. Restart stores in order: Elasticsearch, Redis, PostgreSQL (if needed)
4. Verify health checks pass

