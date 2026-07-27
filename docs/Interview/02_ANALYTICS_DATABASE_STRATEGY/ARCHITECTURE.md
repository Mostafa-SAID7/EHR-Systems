# Analytics Service - Database Architecture

## System Overview

```
┌────────────────────────────────────────────────────────────────────┐
│                        Analytics Service                           │
├────────────────────────────────────────────────────────────────────┤
│                                                                    │
│  ┌──────────────┐    ┌──────────────┐    ┌──────────────┐        │
│  │ Controllers  │    │  Services    │    │  CQRS        │        │
│  │              │    │  (Business   │    │  Handlers    │        │
│  │ HTTP API     │    │  Logic)      │    │              │        │
│  └──────┬───────┘    └──────┬───────┘    └──────┬───────┘        │
│         │                   │                   │               │
│         └───────────────────┼───────────────────┘               │
│                             │                                   │
│         ┌───────────────────▼────────────────────┐              │
│         │    Data Access Layer                   │              │
│         │    (DbContext + Repositories)          │              │
│         └───────────────────┬────────────────────┘              │
│                             │                                   │
└─────────────────────────────┼───────────────────────────────────┘
                              │
        ┌─────────────────────┼─────────────────────┐
        │                     │                     │
        ▼                     ▼                     ▼
    ┌─────────┐        ┌──────────┐        ┌────────────┐
    │   PG    │        │  Redis   │        │ Elasticsearch
    │         │        │          │        │            │
    │Relational       │Cache    │        │ Full-Text  │
    │Data     │        │          │        │ Search     │
    │         │        │  (Opt)   │        │ (Optional) │
    └─────────┘        └──────────┘        └────────────┘
```

---

## The 5 Database Stores

### 1. PostgreSQL (Required)
**Purpose**: Relational data storage  
**Package**: `Npgsql.EntityFrameworkCore.PostgreSQL`  
**Use for**: 
- Analytics reports (normalized schema)
- User activity logs
- System metrics
- Audit events

**Connection**:
```csharp
services.AddPostgresDataAccess<AnalyticsDbContext>(connectionString);
```

**Example entities**:
```csharp
public class AnalyticsReport
{
    public Guid Id { get; set; }
    public string ReportType { get; set; }
    public DateTime CreatedAt { get; set; }
    public string JsonData { get; set; }  // JSONB column
}

public class UserActivity
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Action { get; set; }
    public DateTime Timestamp { get; set; }
}
```

---

### 2. Redis (Optional)
**Purpose**: Distributed caching layer  
**Package**: `StackExchange.Redis`  
**Use for**:
- Query result caching (1-hour TTL)
- Session storage
- Cache-Aside pattern
- Real-time metrics

**Connection**:
```csharp
try {
    services.AddStackExchangeRedisCache(options =>
        options.Configuration = "localhost:6379");
} catch {
    logger.LogWarning("Redis unavailable - continuing without cache");
}
```

**Cache example**:
```csharp
// Set cache
await _cache.SetStringAsync("analytics:report:123", 
    JsonSerializer.Serialize(report),
    new DistributedCacheEntryOptions { 
        AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1) 
    });

// Get cache
var cached = await _cache.GetStringAsync("analytics:report:123");
if (!string.IsNullOrEmpty(cached))
    return JsonSerializer.Deserialize<AnalyticsReport>(cached);

// Fall back to database
return await _dbContext.AnalyticsReports.FindAsync(id);
```

**Benefits**:
- 90%+ cache hit rate
- 10-100x faster than database queries
- Reduces database load
- Improves response times

---

### 3. Elasticsearch (Optional)
**Purpose**: Full-text search and complex queries  
**Package**: `Elastic.Clients.Elasticsearch`  
**Use for**:
- Full-text search on report content
- Audit log queries
- Advanced filtering
- Analytics aggregations

**Connection**:
```csharp
try {
    var settings = new ElasticsearchClientSettings(new Uri("http://localhost:9200"));
    var client = new ElasticsearchClient(settings);
    services.AddSingleton(client);
} catch {
    logger.LogWarning("Elasticsearch unavailable - search disabled");
}
```

**Search example**:
```csharp
var response = await _elasticsearchClient.SearchAsync<AnalyticsReport>(s => s
    .Index("analytics_reports")
    .Query(q => q
        .MultiMatch(mm => mm
            .Query(searchTerm)
            .Fields("reportType", "data"))));

if (response.IsValidResponse)
    return response.Documents;
else
    return Enumerable.Empty<AnalyticsReport>();  // Graceful degradation
```

**Benefits**:
- 80%+ faster than SQL LIKE queries
- Advanced query syntax
- Real-time indexing
- Aggregations support

---

### 4. MongoDB (Optional)
**Purpose**: Document storage for unstructured data  
**Package**: `MongoDB.Driver 2.24.0`  
**Use for**:
- Clinical notes
- Device vitals
- Unstructured analytics data
- Event logs

**Not yet implemented for Analytics** but available pattern:
```csharp
services.AddMongoDataAccess("mongodb://localhost:27017", "ehr_analytics");
```

---

### 5. MySQL (Optional)
**Purpose**: Legacy system integration  
**Package**: `Pomelo.EntityFrameworkCore.MySql`  
**Use for**:
- Billing/claims data
- Insurance integration
- Legacy system synchronization

**Per-service registration** (not in Common):
```csharp
services.AddDbContext<LegacyBillingContext>(options =>
    options.UseMySql(connectionString, 
        ServerVersion.AutoDetect(connectionString)));
```

---

## Data Flow Architecture

### Write Path (Analytics Data Entry)

```
1. HTTP POST /api/analytics/reports
   ├─ Controller receives request
   │
2. Command Handler (CQRS)
   ├─ Validation
   ├─ Create entity
   │
3. PostgreSQL Write
   ├─ Insert AnalyticsReport
   ├─ Trigger OutboxEvent
   │
4. Outbox Event Processor
   ├─ Read OutboxEvent from PostgreSQL
   ├─ Send to Kafka (Outbound)
   ├─ Replicate to Elasticsearch (async)
   ├─ Invalidate Redis cache
   │
5. Response
   ├─ HTTP 201 Created
   └─ Return created report
```

### Read Path (Query Analytics)

```
1. HTTP GET /api/analytics/reports/{id}
   │
2. Query Handler (CQRS)
   ├─ Check Redis cache
   │  ├─ Cache HIT: Return cached data (< 1ms)
   │  └─ Cache MISS: Continue
   │
3. PostgreSQL Read
   ├─ Query AnalyticsReports table
   ├─ Result (5-50ms)
   │
4. Cache Population
   ├─ Store in Redis (1-hour TTL)
   │
5. Response
   ├─ HTTP 200 OK + JSON
   └─ Return to client
```

### Search Path (Full-Text Search)

```
1. HTTP GET /api/analytics/search?q=term
   │
2. Search Query Handler
   │
3. Elasticsearch Query
   ├─ Full-text search on "analytics_reports" index
   ├─ Returns results with relevance scores
   │
4. Response
   ├─ HTTP 200 OK + JSON array
   └─ Return matching reports
```

---

## Entity Relationships

```
AnalyticsDbContext
├── DbSet<AnalyticsReport>
│   └─ Tables: analytics_reports
│      Columns: Id, ReportType, JsonData, CreatedAt, IsArchived
│      Indexes: (ReportType), (CreatedAt)
│
├── DbSet<UserActivity>
│   └─ Tables: user_activities
│      Columns: Id, UserId, Action, Timestamp
│      Indexes: (UserId, Timestamp)
│
├── DbSet<SystemMetrics>
│   └─ Tables: system_metrics
│      Columns: Id, MetricName, Value, Timestamp
│      Indexes: (Timestamp)
│
└── DbSet<OutboxEvent>
    └─ Tables: outbox_events
       Columns: Id, AggregateId, EventType, Payload, CreatedAt, ProcessedAt
       Indexes: (ProcessedAt)
```

---

## Connection String Patterns

### PostgreSQL on Local Development

```
Host=localhost;Port=5432;Database=ehr_analytics;Username=postgres;Password=password;SSL Mode=Disable
```

### PostgreSQL on Replit (via environment variables)

```csharp
static string BuildConnectionString(IConfiguration config)
{
    var connStr = config.GetConnectionString("DefaultConnection");
    if (!string.IsNullOrEmpty(connStr))
        return connStr;

    // Replit environment variables (auto-set)
    var host = config["PGHOST"] ?? "localhost";
    var port = config["PGPORT"] ?? "5432";
    var database = config["PGDATABASE"] ?? "ehr_analytics";
    var user = config["PGUSER"] ?? "postgres";
    var password = config["PGPASSWORD"] ?? "password";

    // SSL only for remote hosts
    var sslMode = host.Contains("localhost") 
        ? "SSL Mode=Disable" 
        : "SSL Mode=Require;Trust Server Certificate=true";

    return $"Host={host};Port={port};Database={database};Username={user};Password={password};{sslMode}";
}
```

---

## CQRS Pattern in Analytics Service

### Commands (Write Operations)

```csharp
// Command definition
public record CreateAnalyticsReportCommand(
    string ReportType,
    Dictionary<string, object> Data,
    DateTime CreatedAt
) : IRequest<Guid>;

// Handler
public class CreateAnalyticsReportCommandHandler 
    : IRequestHandler<CreateAnalyticsReportCommand, Guid>
{
    private readonly AnalyticsDbContext _dbContext;
    private readonly IMediator _mediator;

    public async Task<Guid> Handle(
        CreateAnalyticsReportCommand request,
        CancellationToken cancellationToken)
    {
        var report = new AnalyticsReport
        {
            Id = Guid.NewGuid(),
            ReportType = request.ReportType,
            JsonData = JsonSerializer.Serialize(request.Data),
            CreatedAt = request.CreatedAt
        };

        _dbContext.AnalyticsReports.Add(report);
        await _dbContext.SaveChangesAsync(cancellationToken);

        // Publish domain event
        await _mediator.Publish(
            new AnalyticsReportCreatedEvent(report.Id, request.ReportType),
            cancellationToken);

        return report.Id;
    }
}
```

### Queries (Read Operations)

```csharp
// Query definition
public record GetAnalyticsReportQuery(Guid ReportId) 
    : IRequest<AnalyticsReportDto?>;

// Handler with caching
public class GetAnalyticsReportQueryHandler 
    : IRequestHandler<GetAnalyticsReportQuery, AnalyticsReportDto?>
{
    private readonly AnalyticsDbContext _dbContext;
    private readonly IDistributedCache _cache;

    public async Task<AnalyticsReportDto?> Handle(
        GetAnalyticsReportQuery request,
        CancellationToken cancellationToken)
    {
        // Try cache first
        var cacheKey = $"analytics:report:{request.ReportId}";
        var cached = await _cache.GetStringAsync(cacheKey, cancellationToken);
        if (!string.IsNullOrEmpty(cached))
            return JsonSerializer.Deserialize<AnalyticsReportDto>(cached);

        // Fall back to database
        var report = await _dbContext.AnalyticsReports
            .Where(r => !r.IsArchived && r.Id == request.ReportId)
            .FirstOrDefaultAsync(cancellationToken);

        if (report == null)
            return null;

        // Cache the result
        var dto = MapToDto(report);
        await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(dto),
            new DistributedCacheEntryOptions { 
                AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1) 
            },
            cancellationToken);

        return dto;
    }
}
```

---

## Health Check Pattern

```csharp
public class AnalyticsHealthCheck : IHealthCheck
{
    private readonly AnalyticsDbContext _dbContext;
    private readonly IDistributedCache? _cache;
    private readonly ElasticsearchClient? _elasticsearch;

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var results = new Dictionary<string, object>();

        // PostgreSQL - Required
        try
        {
            await _dbContext.Database.ExecuteSqlRawAsync("SELECT 1", cancellationToken);
            results["PostgreSQL"] = "✓ Healthy";
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("PostgreSQL unavailable", ex);
        }

        // Redis - Optional
        if (_cache != null)
        {
            try
            {
                await _cache.SetStringAsync("health", "ok", cancellationToken);
                var value = await _cache.GetStringAsync("health", cancellationToken);
                results["Redis"] = value == "ok" ? "✓ Healthy" : "✗ Unhealthy";
            }
            catch
            {
                results["Redis"] = "✗ Unavailable";
            }
        }

        // Elasticsearch - Optional
        if (_elasticsearch != null)
        {
            try
            {
                var info = await _elasticsearch.InfoAsync(cancellationToken);
                results["Elasticsearch"] = info.IsValidResponse ? "✓ Healthy" : "✗ Unhealthy";
            }
            catch
            {
                results["Elasticsearch"] = "✗ Unavailable";
            }
        }

        return HealthCheckResult.Healthy("Analytics service operational", results);
    }
}
```

**Endpoint**: `GET /health`

**Response**:
```json
{
  "status": "Healthy",
  "checks": {
    "analytics": {
      "status": "Healthy",
      "data": {
        "PostgreSQL": "✓ Healthy",
        "Redis": "✓ Healthy",
        "Elasticsearch": "✓ Healthy"
      }
    }
  }
}
```

---

## Graceful Degradation Pattern

**The principle**: Service works even if optional stores fail

```csharp
// In Program.cs

var builder = WebApplicationBuilder.CreateBuilder(args);

// PostgreSQL - REQUIRED
var connectionString = BuildConnectionString(builder.Configuration);
builder.Services.AddPostgresDataAccess<AnalyticsDbContext>(connectionString);

// Redis - OPTIONAL
try
{
    var redisConnectionString = builder.Configuration.GetConnectionString("Redis");
    if (!string.IsNullOrEmpty(redisConnectionString))
    {
        builder.Services.AddStackExchangeRedisCache(options =>
            options.Configuration = redisConnectionString);
        logger.LogInformation("Redis initialized successfully");
    }
}
catch (Exception ex)
{
    logger.LogWarning(ex, "Redis initialization failed - continuing without cache");
    // Service continues to work, just without caching
}

// Elasticsearch - OPTIONAL
try
{
    var elasticsearchUrl = builder.Configuration["Elasticsearch:Url"];
    if (!string.IsNullOrEmpty(elasticsearchUrl))
    {
        var settings = new ElasticsearchClientSettings(new Uri(elasticsearchUrl));
        var client = new ElasticsearchClient(settings);
        builder.Services.AddSingleton(client);
        logger.LogInformation("Elasticsearch initialized successfully");
    }
}
catch (Exception ex)
{
    logger.LogWarning(ex, "Elasticsearch initialization failed - search disabled");
    // Service continues to work, just without full-text search
}

var app = builder.Build();
app.Run();
```

**Service behavior**:
- PostgreSQL down → Service fails to start ✗
- Redis down → Service runs slower but works ✓
- Elasticsearch down → Service runs without search ✓
- Both optional stores down → Service still works ✓

---

## Database Indexes Strategy

### PostgreSQL Indexes

```sql
-- Primary key (automatic)
CREATE UNIQUE INDEX ix_analytics_reports_id ON analytics_reports(id);

-- Filtering by report type
CREATE INDEX ix_analytics_reports_type ON analytics_reports(report_type);

-- Sorting by creation date
CREATE INDEX ix_analytics_reports_created_at ON analytics_reports(created_at DESC);

-- Soft delete filter
CREATE INDEX ix_analytics_reports_not_archived ON analytics_reports(id) 
WHERE NOT is_archived;

-- Complex queries
CREATE INDEX ix_user_activities_user_timestamp 
ON user_activities(user_id, timestamp DESC);

-- Full text search column
CREATE INDEX ix_analytics_reports_json_gin 
ON analytics_reports USING GIN(json_data);
```

### Query Optimization

```csharp
// ❌ Slow: Full table scan
var reports = await _dbContext.AnalyticsReports
    .Where(r => r.JsonData.Contains("key"))
    .ToListAsync();

// ✓ Fast: Uses indexes
var reports = await _dbContext.AnalyticsReports
    .Where(r => !r.IsArchived && r.ReportType == "UserActivity")
    .OrderByDescending(r => r.CreatedAt)
    .Take(50)
    .ToListAsync();
```

---

## Performance Benchmarks

| Operation | Without Cache | With Cache | Elasticsearch |
|-----------|---------------|-----------|---------------| |
| Get Report | 50-100ms | < 1ms | N/A |
| List Reports | 200-500ms | 5-10ms | 10-50ms |
| Full-text Search | 1000+ ms | N/A | 50-200ms |
| Index Write | 10-20ms | 15-30ms | 5-10ms |

---

## Extension Points

### Adding New Report Type

1. Create entity in Domain/Entities
2. Add DbSet<T> to AnalyticsDbContext
3. Configure in OnModelCreating
4. Create migration
5. Add service layer
6. Add controller endpoint

### Adding New Cache Layer

1. Inject IDistributedCache
2. Implement cache-aside pattern
3. Set appropriate TTL
4. Test cache invalidation

### Adding New Search Index

1. Configure Elasticsearch index mapping
2. Add indexing to command handlers
3. Create search query handler
4. Add search endpoint

---

## Monitoring & Observability

### Metrics to Track

- PostgreSQL query latency
- Cache hit rate (target: > 85%)
- Elasticsearch search latency
- OutboxEvent processing lag
- Connection pool utilization

### Logging Strategy

```csharp
logger.LogInformation("Analytics report created: {reportId}", reportId);
logger.LogWarning("Cache error: {error}", ex.Message);
logger.LogError(ex, "Database error");
```

### Health Endpoint

```
GET /health
GET /health/detailed
GET /health/ready
```

---

## Security Considerations

1. **Connection strings**: Use environment variables, not hardcoded
2. **Database access**: Minimal permissions per service account
3. **Elasticsearch**: Requires authentication in production
4. **Redis**: No default authentication (use requirepass)
5. **Audit trail**: Log all data access

---

## Disaster Recovery

### Backup Strategy

- PostgreSQL: Daily automated backups
- Redis: Non-persistent (reconstructed from DB)
- Elasticsearch: Replicated across nodes
- OutboxEvents: Retained for 30 days (replay capability)

### Recovery Steps

1. PostgreSQL: Restore from latest backup
2. Redis: Reconstruct cache (slow but safe)
3. Elasticsearch: Reindex from PostgreSQL
4. Outbox: Reprocess events from stored entries

