# Hybrid Approach - Entity Framework + Dapper Integration

## The Best of Both Worlds

This EHR system uses **both EF Core and Dapper together** for optimal performance and productivity.

```
Entity Framework          Dapper
┌─────────────┐          ┌─────────────┐
│  CRUD Ops   │          │  Reports    │
│  Updates    │          │  Analytics  │
│  Complex    │          │  Bulk Ops   │
│  Relations  │          │  Perf Read  │
└──────┬──────┘          └──────┬──────┘
       │                        │
       └────────────┬───────────┘
                    │
            ┌───────▼────────┐
            │  Shared DbContext
            │  Same Connection
            │  Same Transaction
            └────────────────┘
```

---

## Architecture Overview

```csharp
// Single DbContext - both EF and Dapper use it
public class EHRDbContext : DbContext
{
    public DbSet<Patient> Patients { get; set; }
    public DbSet<Appointment> Appointments { get; set; }
    public DbSet<Invoice> Invoices { get; set; }
    // ... other entities
}

// EF for writes and relationships
public class PatientService
{
    private readonly EHRDbContext _context;
    
    public async Task CreateAsync(Patient patient)
    {
        _context.Patients.Add(patient);
        await _context.SaveChangesAsync();
    }
}

// Dapper for complex reads
public class AnalyticsService
{
    private readonly IDapperContext _dapper;
    
    public async Task<DashboardData> GetDashboardAsync()
    {
        return await _dapper.QueryFirstOrDefaultAsync<DashboardData>(
            @"SELECT COUNT(*) as PatientCount, SUM(Amount) as TotalBilled..."
        );
    }
}

// Both services work together in same application
public class EHRApplication
{
    private readonly PatientService _patientService;
    private readonly AnalyticsService _analyticsService;
    
    public async Task RunAsync()
    {
        // Write via EF
        await _patientService.CreateAsync(newPatient);
        
        // Read via Dapper (sees the new patient immediately!)
        var dashboard = await _analyticsService.GetDashboardAsync();
    }
}
```

---

## How They Integrate

### Shared Connection

```csharp
// DapperContext reuses EF's DbConnection
public class DapperContext : IDapperContext
{
    private readonly DbContext _dbContext;
    
    public DapperContext(DbContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    private async Task<IDbConnection> GetOpenConnectionAsync(CancellationToken ct)
    {
        // Get connection from EF's DbContext
        var conn = _dbContext.Database.GetDbConnection();
        
        // Ensure it's open
        if (conn.State != ConnectionState.Open)
            await _dbContext.Database.OpenConnectionAsync(ct);
        
        return conn;
    }
    
    public async Task<IEnumerable<T>> QueryAsync<T>(
        string sql,
        object parameters = null,
        CancellationToken ct = default)
    {
        var conn = await GetOpenConnectionAsync(ct);
        return await conn.QueryAsync<T>(
            new CommandDefinition(sql, parameters, cancellationToken: ct)
        );
    }
}
```

### Shared Transaction

```csharp
// Both EF and Dapper can participate in same transaction
using var transaction = await context.Database.BeginTransactionAsync();
try
{
    // Write via EF
    var patient = new Patient { Name = "Ahmed", MRN = "123456" };
    context.Patients.Add(patient);
    await context.SaveChangesAsync();
    
    // Read via Dapper (sees the EF insert!)
    var stats = await dapperContext.QueryFirstOrDefaultAsync<Stats>(
        "SELECT COUNT(*) as PatientCount FROM Patients",
        transaction: transaction  // Same transaction
    );
    
    await transaction.CommitAsync();
}
catch
{
    await transaction.RollbackAsync();
}
```

---

## Decision Matrix: EF vs Dapper

| Operation | Choose | Why |
|-----------|--------|-----|
| Create patient | EF | Change tracking, validation |
| Update patient | EF | Auto change detection |
| Delete patient | EF | Soft delete via shadow prop |
| Get patient by ID | EF | Simple, relationships |
| Get patient list | EF | Standard CRUD |
| Patient dashboard stats | Dapper | Complex GROUP BY, performance |
| Daily reports | Dapper | Complex JOINs, aggregations |
| Bulk import patients | Dapper | 1M+ rows, direct execute |
| Appointment schedule | EF | Relationships with Patient |
| Billing compliance report | Dapper | Complex aggregations |
| Create invoice | EF | Complex validations |
| Invoice audit trail | Dapper | Complex query |

---

## Practical Workflow

### Scenario 1: Create Patient with Dashboard Update

```csharp
public class PatientWorkflow
{
    private readonly EHRDbContext _context;
    private readonly IDapperContext _dapper;
    
    public async Task OnPatientCreatedAsync(CreatePatientDto dto)
    {
        // Step 1: Create via EF (has validations, relationships)
        var patient = new Patient
        {
            Name = dto.Name,
            MRN = dto.MRN,
            Email = dto.Email
        };
        
        _context.Patients.Add(patient);
        await _context.SaveChangesAsync();  // Patient now in DB
        
        // Step 2: Update dashboard stats via Dapper (direct SQL)
        await _dapper.ExecuteAsync(
            @"UPDATE Dashboard 
              SET PatientCount = PatientCount + 1,
                  LastUpdated = @Now
              WHERE DashboardId = 1",
            new { Now = DateTime.UtcNow }
        );
        
        // Step 3: Get updated stats via Dapper for response
        var stats = await _dapper.QueryFirstOrDefaultAsync<DashboardStats>(
            "SELECT * FROM Dashboard WHERE DashboardId = 1"
        );
        
        return stats;
    }
}
```

### Scenario 2: Complex Report with Details

```csharp
public class ReportService
{
    private readonly EHRDbContext _context;
    private readonly IDapperContext _dapper;
    
    public async Task<DetailedReportDto> GenerateMonthlyReportAsync(int month, int year)
    {
        // Get summary via Dapper (complex query, need speed)
        var summary = await _dapper.QueryFirstOrDefaultAsync<ReportSummary>(
            @"SELECT 
                COUNT(DISTINCT p.Id) as PatientCount,
                COUNT(DISTINCT a.Id) as AppointmentCount,
                SUM(i.Amount) as TotalBilled,
                AVG(i.Amount) as AvgInvoice
              FROM Patients p
              LEFT JOIN Appointments a ON p.Id = a.PatientId 
                  AND MONTH(a.Date) = @Month AND YEAR(a.Date) = @Year
              LEFT JOIN Invoices i ON p.Id = i.PatientId
                  AND MONTH(i.CreatedAt) = @Month AND YEAR(i.CreatedAt) = @Year",
            new { Month = month, Year = year }
        );
        
        // Get detail rows via Dapper (many rows, need speed)
        var details = await _dapper.QueryAsync<ReportDetailRow>(
            @"SELECT p.Id, p.Name, COUNT(a.Id) as ApptCount, SUM(i.Amount) as Billed
              FROM Patients p
              LEFT JOIN Appointments a ON p.Id = a.PatientId
                  AND MONTH(a.Date) = @Month AND YEAR(a.Date) = @Year
              LEFT JOIN Invoices i ON p.Id = i.PatientId
                  AND MONTH(i.CreatedAt) = @Month AND YEAR(i.CreatedAt) = @Year
              GROUP BY p.Id, p.Name
              ORDER BY Billed DESC",
            new { Month = month, Year = year }
        );
        
        return new DetailedReportDto
        {
            Summary = summary,
            Details = details
        };
    }
}
```

### Scenario 3: Audit Trail with EF and Dapper

```csharp
public class AuditService
{
    private readonly EHRDbContext _context;
    private readonly IDapperContext _dapper;
    
    public async Task<AuditHistoryDto> GetPatientAuditTrailAsync(int patientId)
    {
        // Get current patient via EF (with all relationships)
        var patient = await _context.Patients
            .Include(p => p.MedicalRecords)
            .Include(p => p.Appointments)
            .FirstAsync(p => p.Id == patientId);
        
        // Get audit changes via Dapper (complex historical query)
        var changes = await _dapper.QueryAsync<AuditChangeDto>(
            @"SELECT 
                aa.ChangeId,
                aa.ChangedAt,
                aa.ChangedBy,
                aa.FieldName,
                aa.OldValue,
                aa.NewValue
              FROM AuditLog aa
              WHERE aa.EntityId = @PatientId
                AND aa.EntityType = 'Patient'
              ORDER BY aa.ChangedAt DESC",
            new { PatientId = patientId }
        );
        
        return new AuditHistoryDto
        {
            Patient = patient,
            Changes = changes
        };
    }
}
```

---

## Transaction Handling

### Within Same Transaction

```csharp
// Both EF and Dapper participate in same transaction
using var transaction = await context.Database.BeginTransactionAsync();
try
{
    // EF write
    var patient = new Patient { Name = "Ahmed" };
    context.Patients.Add(patient);
    await context.SaveChangesAsync();
    
    // Dapper write - same transaction
    await dapperContext.ExecuteAsync(
        "UPDATE PatientStats SET TotalCount = TotalCount + 1",
        transaction: transaction
    );
    
    // Dapper read - same transaction, sees both writes
    var stats = await dapperContext.QueryFirstOrDefaultAsync<Stats>(
        "SELECT * FROM PatientStats",
        transaction: transaction
    );
    
    await transaction.CommitAsync();
}
catch
{
    await transaction.RollbackAsync();
}
```

---

## Performance Patterns

### Pattern 1: Write EF, Read Dapper

```csharp
// Slow write (EF handles complexity)
var patient = new Patient { /* complex */ };
context.Patients.Add(patient);
await context.SaveChangesAsync();  // 100ms

// Fast read (Dapper direct SQL)
var stats = await dapperContext.QueryAsync<Stat>(
    "SELECT * FROM vw_Stats"  // 10ms
);
```

### Pattern 2: Batch with Dapper, Detail with EF

```csharp
// Bulk insert via Dapper (fast)
await dapperContext.ExecuteAsync(
    @"INSERT INTO Patients (Name, MRN)
      SELECT Name, MRN FROM ImportTemp"  // 500ms for 100k rows
);

// Detailed work via EF (handles relationships)
var patients = await context.Patients
    .Where(p => p.CreatedAt == DateTime.Today)
    .Include(p => p.Appointments)
    .ToListAsync();  // Now work with details
```

### Pattern 3: Caching Layer Pattern

```csharp
public class CachedReportService
{
    private readonly IDapperContext _dapper;
    private readonly IMemoryCache _cache;
    
    public async Task<DashboardData> GetDashboardAsync()
    {
        const string cacheKey = "dashboard_stats";
        
        // Try cache first
        if (_cache.TryGetValue(cacheKey, out DashboardData cached))
            return cached;
        
        // Cache miss - query via Dapper (fast SQL)
        var data = await _dapper.QueryFirstOrDefaultAsync<DashboardData>(
            @"SELECT COUNT(*) as PatientCount,
                     SUM(Amount) as TotalBilled
              FROM Patients p
              LEFT JOIN Invoices i ON p.Id = i.PatientId"
        );
        
        // Cache for 5 minutes
        _cache.Set(cacheKey, data, TimeSpan.FromMinutes(5));
        
        return data;
    }
}
```

---

## Dependency Injection Setup

```csharp
// Program.cs
builder.Services.AddDbContext<EHRDbContext>(options =>
    options.UseSqlServer(connectionString)
);

// Register Dapper context (reuses EF's DbContext)
builder.Services.AddScoped<IDapperContext>(sp =>
    new DapperContext(sp.GetRequiredService<EHRDbContext>())
);

// Register services
builder.Services.AddScoped<IPatientService, PatientService>();
builder.Services.AddScoped<IAnalyticsService, AnalyticsService>();
builder.Services.AddScoped<IReportService, ReportService>();
```

---

## Benefits of Hybrid Approach

| Aspect | Benefit |
|--------|---------|
| **Productivity** | EF handles CRUD quickly, 80% faster development |
| **Performance** | Dapper for reporting/analytics, 10x faster |
| **Maintainability** | Clear separation: EF for business logic, Dapper for reads |
| **Flexibility** | Switch as needs change, not locked into one approach |
| **Testability** | Easy to mock both interfaces |
| **Cost** | No extra licensing, both open source |
| **Community** | Large communities for both technologies |

---

## Challenges and Solutions

### Challenge 1: N+1 Queries with EF

**Problem:** Lazy loading causes extra queries
```csharp
var patients = context.Patients.ToList();
foreach (var p in patients)
{
    var count = p.Appointments.Count;  // N+1!
}
```

**Solution:** Use Include or Dapper
```csharp
// Option A: Include
var patients = context.Patients
    .Include(p => p.Appointments)
    .ToList();

// Option B: Switch to Dapper for this read
var appointments = await dapperContext.QueryAsync<AppointmentCount>(
    @"SELECT PatientId, COUNT(*) as Count
      FROM Appointments GROUP BY PatientId"
);
```

### Challenge 2: Change Tracking Memory

**Problem:** Large result sets consume memory
```csharp
var allPatients = context.Patients.ToList();  // All tracked, high memory
```

**Solution:** Use Dapper for read-only
```csharp
var allPatients = await dapperContext.QueryAsync<Patient>(
    "SELECT * FROM Patients"  // No tracking, low memory
);
```

### Challenge 3: Stale Cache with Dapper Updates

**Problem:** EF cache doesn't know about Dapper updates
```csharp
var patient = context.Patients.First(p => p.Id == 1);
await dapperContext.ExecuteAsync(
    "UPDATE Patients SET Name = @Name WHERE Id = @Id",
    new { Name = "NewName", Id = 1 }
);
Console.WriteLine(patient.Name);  // Still old name!
```

**Solution:** Explicitly reload or clear cache
```csharp
await dapperContext.ExecuteAsync(sql);
await context.Entry(patient).ReloadAsync();  // Reload from DB
Console.WriteLine(patient.Name);  // Now updated
```

---

## Monitoring and Profiling

```csharp
// Enable query logging
var optionsBuilder = new DbContextOptionsBuilder<EHRDbContext>();
optionsBuilder
    .UseSqlServer(connectionString)
    .LogTo(Console.WriteLine, LogLevel.Information);

// Log Dapper queries
var sw = Stopwatch.StartNew();
var results = await dapperContext.QueryAsync<T>(sql);
sw.Stop();
Console.WriteLine($"Dapper query took {sw.ElapsedMilliseconds}ms");
```

---

## Migration Strategy

**From EF-only to Hybrid:**

1. **Identify bottlenecks** - Which queries are slow?
2. **Profile** - Measure with EF then Dapper
3. **Switch selectively** - Replace only necessary queries
4. **Test thoroughly** - Ensure correctness
5. **Monitor** - Watch performance improvement

**From Dapper-only to Hybrid:**

1. **Start with CRUD** - Use EF for create/update/delete
2. **Keep complex reads** - Dapper for reports
3. **Leverage relationships** - EF for navigation properties
4. **Gradual migration** - Don't refactor all at once

---

## Interview Questions

**Q: Why use both EF and Dapper?**

A: EF excels at CRUD and relationships, Dapper excels at performance-critical reads. Together they provide best productivity and performance.

**Q: How do they share transactions?**

A: Dapper uses EF's DbConnection. By passing the same transaction object, both operations participate in same ACID guarantee.

**Q: Division of responsibility?**

A:
- EF: Writes, updates, relationships, complex business logic
- Dapper: Reads, reports, analytics, bulk operations

**Q: Performance impact of hybrid?**

A: No penalty. Each tool does what it's best at, resulting in optimal performance for each operation type.

**Q: Can they conflict?**

A: Potentially if EF cache doesn't know about Dapper updates. Solution: Use AsNoTracking() or reload explicitly.

---

## Related Documentation

- **Entity Framework:** ../EntityFramework/README.md
- **Dapper:** ../Dapper/README.md
- **Comparison:** ../orm-comparison.md
- **EHR Examples:** ehr-practical-examples.md
