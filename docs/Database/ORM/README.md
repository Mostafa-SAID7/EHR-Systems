# ORM - Object-Relational Mapping Guide

## What is ORM?

An ORM bridges your C# objects and database tables, converting between:
- **Objects** (in-memory C# classes)
- **Relations** (database tables/rows)

```
User Class (C#)  ←→  ORM Layer  ←→  Users Table (DB)
┌─────────┐           ┌────────┐           ┌──────┐
│ Id      │           │ Maps   │           │ Id   │
│ Email   │───────────│ objects│───────────│ Email│
│ Name    │           │ to SQL │           │ Name │
└─────────┘           └────────┘           └──────┘
```

---

## Folder Structure

```
Database/ORM/
├── README.md (This file - Overview)
├── orm-comparison.md (EF vs Dapper vs Raw SQL)
│
├── EntityFramework/
│   ├── README.md
│   ├── why-entity-framework.md
│   ├── dbcontext-fundamentals.md
│   ├── change-tracking.md
│   ├── loading-strategies.md
│   ├── migrations.md
│   ├── query-optimization.md
│   ├── transactions-concurrency.md
│   ├── patterns.md
│   └── interview-qa.md
│
├── Dapper/
│   ├── README.md
│   ├── why-dapper.md
│   ├── dapper-fundamentals.md
│   ├── query-patterns.md
│   ├── advanced-features.md
│   ├── performance-tuning.md
│   └── interview-qa.md
│
└── Hybrid/
    ├── README.md
    ├── ef-dapper-integration.md
    └── ehr-practical-examples.md
```

---

## ORMs in This Project

This EHR system uses a **hybrid approach:**

### Entity Framework Core (Primary)
- Domain models and relationships
- Migrations and schema versioning
- Change tracking
- Complex queries with LINQ

### Dapper (Complementary)
- Complex reporting queries
- Performance-critical reads
- Bulk operations
- Direct SQL when needed

**Why both?**
- EF: Great for CRUD and relationships
- Dapper: Great for performance-critical reads
- Together: Best of both worlds

---

## Quick Comparison

| Feature | EF Core | Dapper | Raw SQL |
|---------|---------|--------|---------|
| **Learning Curve** | Steep | Easy | Easy |
| **Performance** | Good | Excellent | Excellent |
| **Type Safety** | ✅ Strong | ⚠️ Partial | ❌ None |
| **Flexibility** | Good | Excellent | Excellent |
| **Change Tracking** | ✅ Auto | ❌ Manual | ❌ None |
| **Relationships** | ✅ Built-in | ❌ None | ❌ None |
| **Migrations** | ✅ Automatic | ❌ Manual | ❌ Manual |
| **LINQ Queries** | ✅ Yes | ❌ No | ❌ No |
| **Async Support** | ✅ Excellent | ✅ Excellent | ⚠️ Limited |

---

## When to Use Each

### Use Entity Framework When:
- ✅ Creating new services (fresh development)
- ✅ Complex relationships between entities
- ✅ Need automatic change tracking
- ✅ Schema migrations needed
- ✅ Standard CRUD operations
- ✅ Want type-safe LINQ queries

### Use Dapper When:
- ✅ Reading complex reports (5+ JOINs)
- ✅ Performance is critical
- ✅ Bulk inserts/updates (1M+ rows)
- ✅ Direct SQL optimization needed
- ✅ Stored procedures
- ✅ Complex aggregations/analytics

### Use Raw SQL When:
- ✅ Database-specific features
- ✅ Extreme performance requirements
- ✅ Complex dynamic queries
- ✅ Direct database operations

---

## Learning Path

### Day 1: Fundamentals
- [ ] Read: orm-comparison.md (understand each ORM)
- [ ] Read: EntityFramework/why-entity-framework.md
- [ ] Read: Dapper/why-dapper.md

### Day 2-3: Entity Framework Deep Dive
- [ ] Study: EntityFramework/ (all files)
- [ ] Focus: DbContext, change tracking, loading strategies
- [ ] Practice: Writing EF queries

### Day 4-5: Dapper Deep Dive
- [ ] Study: Dapper/ (all files)
- [ ] Focus: Query patterns, performance, stored procedures
- [ ] Practice: Writing Dapper queries

### Day 6: Integration & Hybrid Patterns
- [ ] Study: Hybrid/ (integration approach)
- [ ] Read: ehr-practical-examples.md
- [ ] Understand: How EF and Dapper work together

### Day 7: Interview Prep
- [ ] Study: EntityFramework/interview-qa.md
- [ ] Study: Dapper/interview-qa.md
- [ ] Practice: Explaining when to use each

---

## In the EHR Codebase

### EntityFramework Used For:
```csharp
// Domain models with relationships
public class Patient
{
    public int Id { get; set; }
    public string MRN { get; set; }
    public ICollection<Appointment> Appointments { get; set; }
    public ICollection<MedicalRecord> Records { get; set; }
}

// CRUD operations
var patient = await context.Patients
    .Include(p => p.Appointments)
    .FirstAsync(p => p.Id == patientId);

// Migrations
dotnet ef migrations add AddPatientPhoneNumber
```

### Dapper Used For:
```csharp
// Complex reporting queries
var dashboardData = await dapperContext.QueryAsync<DashboardDto>(
    @"SELECT p.Id, p.Name, COUNT(a.Id) as AppointmentCount,
             SUM(i.Amount) as TotalBilled
      FROM Patients p
      LEFT JOIN Appointments a ON p.Id = a.PatientId
      LEFT JOIN Invoices i ON p.Id = i.PatientId
      WHERE p.Status = @Status
      GROUP BY p.Id, p.Name",
    new { Status = "Active" }
);

// Bulk operations
var rowsAffected = await dapperContext.ExecuteAsync(
    @"UPDATE Patients SET LastVisitDate = @Today WHERE Status = @Status",
    new { Today = DateTime.Today, Status = "Inactive" }
);
```

### Both Share DbContext
```csharp
// DapperContext reuses EF's connection
public class DapperContext : IDapperContext
{
    private readonly DbContext _dbContext;
    
    public DapperContext(DbContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    private async Task<IDbConnection> GetOpenConnectionAsync()
    {
        var conn = _dbContext.Database.GetDbConnection();
        if (conn.State != ConnectionState.Open)
            await _dbContext.Database.OpenConnectionAsync();
        return conn;
    }
}
```

---

## Key Concepts to Understand

### ORM Decision Tree

```
Question: What's the query?
│
├─ Simple CRUD (Create, Read, Update, Delete)?
│  └─ Use: Entity Framework
│     - Type-safe, auto-tracking, relationships
│
├─ Complex report (5+ tables, aggregations)?
│  └─ Use: Dapper
│     - Raw SQL, performance, flexibility
│
├─ Bulk operation (1M+ rows)?
│  └─ Use: Dapper
│     - Direct SQL, fast, efficient
│
└─ Need database-specific feature?
   └─ Use: Raw SQL
      - Full control, best performance
```

---

## Common Patterns in EHR

### Pattern 1: EF for writes, Dapper for reads

```csharp
// Write: EF Core
public async Task CreatePatientAsync(Patient patient)
{
    await context.Patients.AddAsync(patient);
    await context.SaveChangesAsync();
}

// Read: Dapper
public async Task<PatientReportDto> GetPatientReportAsync(int id)
{
    return await dapperContext.QueryFirstOrDefaultAsync<PatientReportDto>(
        "SELECT * FROM vw_PatientReport WHERE PatientId = @Id",
        new { Id = id }
    );
}
```

### Pattern 2: EF for relationships, Dapper for performance

```csharp
// EF: Get patient with all related data
var patient = await context.Patients
    .Include(p => p.Appointments)
    .Include(p => p.MedicalRecords)
    .FirstAsync();

// Dapper: Get patient stats for dashboard
var stats = await dapperContext.QueryFirstOrDefaultAsync<PatientStatsDto>(
    @"SELECT 
        COUNT(DISTINCT a.Id) as AppointmentCount,
        COUNT(DISTINCT r.Id) as RecordCount,
        MAX(a.AppointmentDate) as LastAppointment
      FROM Patients p
      LEFT JOIN Appointments a ON p.Id = a.PatientId
      LEFT JOIN MedicalRecords r ON p.Id = r.PatientId
      WHERE p.Id = @Id",
    new { Id = patientId }
);
```

---

## Interview Questions by ORM

### Entity Framework
- What is DbContext and why scoped?
- How does change tracking work?
- What's N+1 query problem?
- Include vs ThenInclude vs Select?
- Optimistic vs pessimistic locking?

**See:** EntityFramework/interview-qa.md

### Dapper
- When to use Dapper over EF?
- How to map query results?
- What's multi-mapping?
- Performance advantages?
- Parameterization and SQL injection?

**See:** Dapper/interview-qa.md

### Hybrid Approach
- Why use both EF and Dapper?
- How do they integrate?
- Division of responsibility?
- When to switch between them?

**See:** Hybrid/ef-dapper-integration.md

---

## Navigation Tips

**If you want to learn:**
- ✅ **EF Core:** Start with EntityFramework/README.md
- ✅ **Dapper:** Start with Dapper/README.md
- ✅ **Both together:** Start with orm-comparison.md then Hybrid/README.md
- ✅ **Practical code:** See Hybrid/ehr-practical-examples.md
- ✅ **Interview prep:** Each ORM folder has interview-qa.md

**If you have a question:**
- "What's N+1?" → EntityFramework/loading-strategies.md
- "When use Dapper?" → Dapper/why-dapper.md
- "EF performance bad?" → EntityFramework/query-optimization.md
- "How to do bulk insert?" → Dapper/advanced-features.md
- "Should DbContext be singleton?" → EntityFramework/dbcontext-fundamentals.md

---

## Next Steps

1. **Understand the landscape:** Read orm-comparison.md
2. **Pick your focus:**
   - Primary developer? → Study EntityFramework/
   - Performance engineer? → Study Dapper/
   - Full stack? → Study both
3. **Learn the codebase:** Check Hybrid/ehr-practical-examples.md
4. **Interview prep:** Study question files before interviews

---

## Related Documentation

- **Database Design:** See ../Design/database-design.md
- **SQL Performance:** See ../SQL/sql-performance.md
- **EHR Schema:** See ../Schema/ehr-complete-schema.md
