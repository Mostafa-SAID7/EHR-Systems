# Analytics Service - Database Migration Guide

## Overview

This guide walks through implementing the polyglot database strategy for the Analytics service following the EHR platform architecture.

**Time to complete**: 4-6 hours  
**Complexity**: Medium  
**Dependencies**: PostgreSQL (required), Redis/Elasticsearch (optional but recommended)

---

## Phase 1: PostgreSQL Setup (Required)

### Step 1.1: Update appsettings.json

**File**: `appsettings.json`

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=ehr_analytics;Username=postgres;Password=password;SSL Mode=Disable"
  }
}
```

**For Replit**: Leave empty and use environment variables:
```
PGHOST=localhost
PGPORT=5432
PGDATABASE=ehr_analytics
PGUSER=postgres
PGPASSWORD=password
```

---

### Step 1.2: Create AnalyticsDbContext

**File**: `Data/AnalyticsDbContext.cs`

```csharp
using Microsoft.EntityFrameworkCore;
using EHRPlatform.Common.Data;
using EHRPlatform.Services.Analytics.Domain.Entities;

namespace EHRPlatform.Services.Analytics.Data
{
    public class AnalyticsDbContext : BaseDbContext
    {
        public DbSet<AnalyticsReport> AnalyticsReports { get; set; }
        public DbSet<UserActivity> UserActivities { get; set; }
        public DbSet<SystemMetrics> SystemMetrics { get; set; }
        
        // Required for OutboxEvent pattern (from BaseDbContext)
        public DbSet<OutboxEvent> OutboxEvents { get; set; }

        public AnalyticsDbContext(DbContextOptions<AnalyticsDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure AnalyticsReport
            modelBuilder.Entity<AnalyticsReport>(entity =>
            {
                entity.ToTable("analytics_reports");
                entity.HasKey(e => e.Id);
                
                entity.Property(e => e.ReportType)
                    .HasConversion<string>()
                    .IsRequired();
                
                entity.Property(e => e.Data)
                    .HasColumnType("jsonb");  // PostgreSQL JSON
                
                entity.HasIndex(e => e.CreatedAt)
                    .HasName("ix_analytics_reports_created_at");
                
                entity.HasIndex(e => e.ReportType)
                    .HasName("ix_analytics_reports_type");
            });

            // Configure UserActivity
            modelBuilder.Entity<UserActivity>(entity =>
            {
                entity.ToTable("user_activities");
                entity.HasKey(e => e.Id);
                
                entity.HasIndex(e => new { e.UserId, e.Timestamp })
                    .HasName("ix_user_activities_user_timestamp");
            });

            // Configure SystemMetrics
            modelBuilder.Entity<SystemMetrics>(entity =>
            {
                entity.ToTable("system_metrics");
                entity.HasKey(e => e.Id);
                
                entity.HasIndex(e => e.Timestamp)
                    .HasName("ix_system_metrics_timestamp");
            });

            // Soft delete filter (from BaseDbContext)
            ConfigureSoftDeleteFilter(modelBuilder);
        }
    }
}
```

---

### Step 1.3: Register in Program.cs

**File**: `Program.cs`

```csharp
using EHRPlatform.Common.Extensions;
using EHRPlatform.Services.Analytics.Data;

var builder = WebApplicationBuilder.CreateBuilder(args);

// Add services
builder.Services.AddControllers();

// PostgreSQL (Required)
var connectionString = BuildConnectionString(builder.Configuration);
builder.Services
    .AddPostgresDataAccess<AnalyticsDbContext>(connectionString)
    .AddDataAccess<AnalyticsDbContext>(connectionString);

// Build app
var app = builder.Build();

// Apply migrations
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AnalyticsDbContext>();
    await dbContext.Database.MigrateAsync();
}

app.MapControllers();
app.Run();

// Helper to build connection string
static string BuildConnectionString(IConfiguration config)
{
    var connStr = config.GetConnectionString("DefaultConnection");
    if (!string.IsNullOrEmpty(connStr))
        return connStr;

    // Replit environment variables
    var host = config["PGHOST"] ?? "localhost";
    var port = config["PGPORT"] ?? "5432";
    var database = config["PGDATABASE"] ?? "ehr_analytics";
    var user = config["PGUSER"] ?? "postgres";
    var password = config["PGPASSWORD"] ?? "password";

    var sslMode = host.Contains("localhost") 
        ? "SSL Mode=Disable" 
        : "SSL Mode=Require;Trust Server Certificate=true";

    return $"Host={host};Port={port};Database={database};Username={user};Password={password};{sslMode}";
}
```

---

### Step 1.4: Create EF Core Migration

```bash
cd backend/src/EHRPlatform.Services.Analytics

# Create migration
dotnet ef migrations add InitialCreate \
    --context AnalyticsDbContext \
    --output-dir Data/Migrations

# Apply to database
dotnet ef database update \
    --context AnalyticsDbContext
```

**Expected files created**:
- `Data/Migrations/[timestamp]_InitialCreate.cs`
- `Data/Migrations/AnalyticsDbContextModelSnapshot.cs`

---

## Phase 2: Redis Caching (Optional but Recommended)

### Step 2.1: Update appsettings.json

```json
{
  "ConnectionStrings": {
    "Redis": "localhost:6379"
  }
}
```

---

### Step 2.2: Create Caching Service

**File**: `Application/Services/AnalyticsCacheService.cs`

```csharp
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace EHRPlatform.Services.Analytics.Application.Services
{
    public class AnalyticsCacheService
    {
        private readonly IDistributedCache _cache;
        private const string CacheKeyPrefix = "analytics:";

        public AnalyticsCacheService(IDistributedCache cache)
        {
            _cache = cache;
        }

        public async Task<T?> GetAsync<T>(string key)
        {
            try
            {
                var cachedData = await _cache.GetStringAsync(CacheKeyPrefix + key);
                if (string.IsNullOrEmpty(cachedData))
                    return default;

                return JsonSerializer.Deserialize<T>(cachedData);
            }
            catch (Exception ex)
            {
                // Graceful degradation: log and return null
                // (Caller should fetch from DB)
                return default;
            }
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
        {
            try
            {
                var serialized = JsonSerializer.Serialize(value);
                var options = new DistributedCacheEntryOptions();
                
                if (expiration.HasValue)
                    options.AbsoluteExpirationRelativeToNow = expiration;
                else
                    options.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1);

                await _cache.SetStringAsync(CacheKeyPrefix + key, serialized, options);
            }
            catch
            {
                // Silently fail - cache is optional
            }
        }

        public async Task RemoveAsync(string key)
        {
            try
            {
                await _cache.RemoveAsync(CacheKeyPrefix + key);
            }
            catch
            {
                // Silently fail
            }
        }
    }
}
```

---

### Step 2.3: Register in Program.cs

```csharp
// Redis (Optional - graceful degradation)
try
{
    var redisConnectionString = builder.Configuration.GetConnectionString("Redis");
    if (!string.IsNullOrEmpty(redisConnectionString))
    {
        builder.Services.AddStackExchangeRedisCache(options =>
            options.Configuration = redisConnectionString);
        
        builder.Services.AddScoped<AnalyticsCacheService>();
        logger.LogInformation("Redis cache initialized");
    }
}
catch (Exception ex)
{
    logger.LogWarning(ex, "Failed to initialize Redis cache - continuing without caching");
}
```

---

## Phase 3: Elasticsearch Search (Optional)

### Step 3.1: Update appsettings.json

```json
{
  "Elasticsearch": {
    "Url": "http://localhost:9200"
  }
}
```

---

### Step 3.2: Create Search Service

**File**: `Application/Services/AnalyticsSearchService.cs`

```csharp
using Elastic.Clients.Elasticsearch;
using EHRPlatform.Services.Analytics.Domain.Entities;

namespace EHRPlatform.Services.Analytics.Application.Services
{
    public class AnalyticsSearchService
    {
        private readonly ElasticsearchClient _client;

        public AnalyticsSearchService(ElasticsearchClient client)
        {
            _client = client;
        }

        public async Task<IEnumerable<AnalyticsReport>> SearchReportsAsync(
            string searchTerm, 
            int pageSize = 10)
        {
            try
            {
                var response = await _client.SearchAsync<AnalyticsReport>(s => s
                    .Index("analytics_reports")
                    .Query(q => q
                        .MultiMatch(mm => mm
                            .Query(searchTerm)
                            .Fields("reportType", "data"))));

                if (!response.IsValidResponse)
                    throw new InvalidOperationException("Search failed");

                return response.Documents;
            }
            catch (Exception ex)
            {
                // Graceful degradation: search is optional
                return Enumerable.Empty<AnalyticsReport>();
            }
        }

        public async Task IndexReportAsync(AnalyticsReport report)
        {
            try
            {
                await _client.IndexAsync(report, i => i
                    .Index("analytics_reports")
                    .Id(report.Id.ToString()));
            }
            catch
            {
                // Silently fail - search is optional
            }
        }
    }
}
```

---

### Step 3.3: Register in Program.cs

```csharp
// Elasticsearch (Optional - graceful degradation)
try
{
    var elasticsearchUrl = builder.Configuration["Elasticsearch:Url"];
    if (!string.IsNullOrEmpty(elasticsearchUrl))
    {
        var settings = new ElasticsearchClientSettings(new Uri(elasticsearchUrl));
        var client = new ElasticsearchClient(settings);
        
        builder.Services.AddSingleton(client);
        builder.Services.AddScoped<AnalyticsSearchService>();
        logger.LogInformation("Elasticsearch initialized");
    }
}
catch (Exception ex)
{
    logger.LogWarning(ex, "Failed to initialize Elasticsearch - continuing without search");
}
```

---

## Phase 4: Health Checks

### Step 4.1: Create Health Check

**File**: `Infrastructure/HealthChecks/AnalyticsHealthCheck.cs`

```csharp
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace EHRPlatform.Services.Analytics.Infrastructure.HealthChecks
{
    public class AnalyticsHealthCheck : IHealthCheck
    {
        private readonly AnalyticsDbContext _dbContext;
        private readonly IDistributedCache? _cache;
        private readonly ElasticsearchClient? _elasticsearch;

        public AnalyticsHealthCheck(
            AnalyticsDbContext dbContext,
            IDistributedCache? cache = null,
            ElasticsearchClient? elasticsearch = null)
        {
            _dbContext = dbContext;
            _cache = cache;
            _elasticsearch = elasticsearch;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            var results = new Dictionary<string, object>();

            // PostgreSQL (Required)
            try
            {
                await _dbContext.Database.ExecuteSqlRawAsync(
                    "SELECT 1", cancellationToken);
                results["PostgreSQL"] = "Healthy";
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy("PostgreSQL check failed", ex);
            }

            // Redis (Optional)
            if (_cache != null)
            {
                try
                {
                    var testKey = "health-check";
                    await _cache.SetStringAsync(testKey, "ok", cancellationToken);
                    var value = await _cache.GetStringAsync(testKey, cancellationToken);
                    await _cache.RemoveAsync(testKey, cancellationToken);
                    
                    results["Redis"] = value == "ok" ? "Healthy" : "Failed";
                }
                catch
                {
                    results["Redis"] = "Unavailable";
                }
            }

            // Elasticsearch (Optional)
            if (_elasticsearch != null)
            {
                try
                {
                    var response = await _elasticsearch.InfoAsync(cancellationToken);
                    results["Elasticsearch"] = response.IsValidResponse ? "Healthy" : "Failed";
                }
                catch
                {
                    results["Elasticsearch"] = "Unavailable";
                }
            }

            return HealthCheckResult.Healthy("Analytics service is healthy", results);
        }
    }
}
```

---

### Step 4.2: Register Health Checks in Program.cs

```csharp
builder.Services
    .AddHealthChecks()
    .AddCheck<AnalyticsHealthCheck>("analytics");

// In app configuration:
app.MapHealthChecks("/health");
```

---

## Phase 5: Data Migration

### Step 5.1: Create Migration Script

If migrating from old Analytics database:

**File**: `Data/Migrations/MigrateAnalyticsData.cs`

```csharp
public static class DataMigration
{
    public static async Task MigrateFromLegacyAsync(
        AnalyticsDbContext newContext,
        ILogger logger)
    {
        try
        {
            // Example: Migrate reports from old store
            var legacyReports = await FetchLegacyReportsAsync();
            
            foreach (var legacyReport in legacyReports)
            {
                var newReport = MapLegacyToNew(legacyReport);
                newContext.AnalyticsReports.Add(newReport);
            }

            await newContext.SaveChangesAsync();
            logger.LogInformation("Migrated {count} reports", legacyReports.Count);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Migration failed");
            throw;
        }
    }

    private static AnalyticsReport MapLegacyToNew(LegacyReport legacy)
    {
        return new AnalyticsReport
        {
            Id = Guid.NewGuid(),
            ReportType = legacy.Type,
            Data = JsonSerializer.Serialize(legacy.Content),
            CreatedAt = legacy.CreatedDate,
            IsArchived = legacy.IsDeleted
        };
    }
}
```

---

### Step 5.2: Execute Migration

```csharp
// In Program.cs, after migrations
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AnalyticsDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    
    // Optional: Run data migration
    // await DataMigration.MigrateFromLegacyAsync(dbContext, logger);
}
```

---

## Testing & Verification

### Unit Test Example

**File**: `Tests/AnalyticsCacheServiceTests.cs`

```csharp
[TestClass]
public class AnalyticsCacheServiceTests
{
    [TestMethod]
    public async Task SetAndGet_ShouldReturnCachedValue()
    {
        // Arrange
        var mockCache = new Mock<IDistributedCache>();
        var service = new AnalyticsCacheService(mockCache.Object);
        var testData = new { Key = "value" };

        // Act
        await service.SetAsync("test", testData);
        var result = await service.GetAsync<dynamic>("test");

        // Assert
        Assert.IsNotNull(result);
    }

    [TestMethod]
    public async Task GetAsync_WhenCacheFails_ShouldReturnNull()
    {
        // Arrange
        var mockCache = new Mock<IDistributedCache>();
        mockCache.Setup(c => c.GetStringAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Cache unavailable"));
        
        var service = new AnalyticsCacheService(mockCache.Object);

        // Act
        var result = await service.GetAsync<dynamic>("test");

        // Assert
        Assert.IsNull(result);  // Graceful degradation
    }
}
```

---

### Integration Test Example

**File**: `Tests/AnalyticsDbContextTests.cs`

```csharp
[TestClass]
public class AnalyticsDbContextTests : IAsyncLifetime
{
    private IServiceProvider _serviceProvider;
    private AnalyticsDbContext _dbContext;

    public async Task InitializeAsync()
    {
        var builder = new ServiceCollection()
            .AddDbContext<AnalyticsDbContext>(options =>
                options.UseSqlite("Data Source=:memory:"));

        _serviceProvider = builder.BuildServiceProvider();
        _dbContext = _serviceProvider.GetRequiredService<AnalyticsDbContext>();
        
        await _dbContext.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync()
    {
        await _dbContext.DisposeAsync();
        _serviceProvider?.Dispose();
    }

    [TestMethod]
    public async Task AnalyticsReport_ShouldBeCreated()
    {
        // Arrange
        var report = new AnalyticsReport
        {
            Id = Guid.NewGuid(),
            ReportType = "UserActivity",
            Data = "{}",
            CreatedAt = DateTime.UtcNow
        };

        // Act
        _dbContext.AnalyticsReports.Add(report);
        await _dbContext.SaveChangesAsync();

        // Assert
        var saved = await _dbContext.AnalyticsReports.FindAsync(report.Id);
        Assert.IsNotNull(saved);
        Assert.AreEqual("UserActivity", saved.ReportType);
    }
}
```

---

## Verification Checklist

- [ ] PostgreSQL connection string configured
- [ ] AnalyticsDbContext created with entities
- [ ] Migrations created and applied
- [ ] Program.cs registers all databases with try/catch
- [ ] Health check endpoint responds correctly
- [ ] Tests pass for PostgreSQL operations
- [ ] Redis connection optional (graceful degradation)
- [ ] Elasticsearch connection optional (graceful degradation)
- [ ] Caching reduces query latency
- [ ] Search results returned from Elasticsearch
- [ ] Documentation updated

---

## Troubleshooting

### PostgreSQL Connection Issues

```
Error: "Unable to connect to Postgres"

Solutions:
1. Check connection string: Host, port, database, username, password
2. Verify PostgreSQL is running: psql -U postgres
3. For Replit, set environment variables: PGHOST, PGPORT, etc.
4. Check firewall/network rules
```

### Redis Connection Failures

```
Error: "Failed to initialize Redis"

Solution: Check logs - this is graceful degradation
- Service will continue without caching
- Performance will be lower but functionality preserved
- Look for "WARNING" logs about Redis
```

### Elasticsearch Issues

```
Error: "Invalid response from Elasticsearch"

Solutions:
1. Verify Elasticsearch is running
2. Check URL in appsettings.json
3. Ensure index exists
4. Check network connectivity
5. Note: Service continues without search functionality
```

---

## Performance Optimization Tips

1. **Cache frequently accessed reports** (1-hour TTL)
2. **Index large datasets** in Elasticsearch
3. **Use database indexes** on ReportType, CreatedAt
4. **Batch write operations** when possible
5. **Monitor query performance** with metrics

---

## Rollback Plan

If migration fails:

```bash
# Rollback migrations
dotnet ef database update [PreviousMigration] \
    --context AnalyticsDbContext

# Or remove all migrations
dotnet ef database update 0 \
    --context AnalyticsDbContext
```

---

## Next Steps

1. Complete all phases in order
2. Run tests locally
3. Test graceful degradation (disable each optional store)
4. Deploy to Replit
5. Monitor health checks
6. Add monitoring/alerts for connection issues

