# Entity Framework Core - Complete Guide

## Why Entity Framework?

Entity Framework is an **ORM that automatically handles object-to-database mapping**.

### Advantages ✅
- **Type-safe LINQ** - Compile-time checking, IntelliSense
- **Change tracking** - Auto-detect what changed
- **Migrations** - Version control for schema
- **Relationships** - Foreign keys handled automatically
- **Async/await** - Built-in async support
- **Multiple databases** - SQL Server, MySQL, PostgreSQL, etc.

### Trade-offs ❌
- **Performance** - Slower than raw SQL for complex queries
- **Learning curve** - More concepts to understand
- **Memory** - Change tracking uses memory
- **Magic** - Hidden queries can surprise you (N+1)

---

## Folder Contents

| File | Focus |
|------|-------|
| **why-entity-framework.md** | EF benefits, when to use, alternatives |
| **dbcontext-fundamentals.md** | DbContext, lifecycle, scoped vs singleton |
| **change-tracking.md** | How EF detects changes, performance impact |
| **loading-strategies.md** | Lazy/Eager/Explicit, N+1 problem, solutions |
| **migrations.md** | Schema versioning, creating and rolling back |
| **query-optimization.md** | Performance tuning, indexes, execution plans |
| **transactions-concurrency.md** | ACID, optimistic/pessimistic locking |
| **patterns.md** | Repository, Unit of Work, CQRS patterns |
| **interview-qa.md** | 30 critical interview questions with answers |

---

## Quick Start (5 minutes)

### 1. Setup DbContext
```csharp
public class EHRDbContext : DbContext
{
    public DbSet<Patient> Patients { get; set; }
    public DbSet<Appointment> Appointments { get; set; }
    
    public EHRDbContext(DbContextOptions options) : base(options) { }
}
```

### 2. Register in DI
```csharp
builder.Services.AddDbContext<EHRDbContext>(options =>
    options.UseSqlServer("connection-string")
);
```

### 3. Create Migration
```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

### 4. Use in Service
```csharp
public class PatientService
{
    private readonly EHRDbContext _context;
    
    public async Task<Patient> GetPatientAsync(int id)
    {
        return await _context.Patients
            .Include(p => p.Appointments)  // Prevent N+1
            .FirstAsync(p => p.Id == id);
    }
}
```

---

## Core Concepts

### DbContext
Your session to the database. **Must be scoped** (new per request).

```csharp
using (var context = new EHRDbContext(options))
{
    // Your queries here
    var patient = await context.Patients.FirstAsync();
} // Disposed automatically
```

### Change Tracking
EF watches your entities and generates UPDATE/INSERT/DELETE.

```csharp
var patient = context.Patients.First();
patient.Name = "Changed";
await context.SaveChangesAsync();
// EF: UPDATE Patients SET Name = 'Changed' WHERE Id = X
```

### Loading Strategies
How to load related data efficiently.

```csharp
// Eager - Load upfront
var patient = context.Patients
    .Include(p => p.Appointments)
    .First();

// Explicit - Load later
var patient = context.Patients.First();
await context.Entry(patient)
    .Collection(p => p.Appointments)
    .LoadAsync();

// Lazy - Auto-load (❌ Causes N+1)
var patient = context.Patients.First();
var count = patient.Appointments.Count; // Separate query!
```

### Migrations
Version control for your database schema.

```bash
dotnet ef migrations add AddPhoneColumn
dotnet ef database update
dotnet ef database update PreviousMigration  # Rollback
```

---

## Most Critical Interview Questions

**Q1: What's DbContext and why scoped?**
- DbContext is your session to database
- Scoped = new per request, disposed after
- Never singleton (memory leaks, thread-safety issues)

**Q2: What's N+1 query problem?**
- 1 query to load users + N queries to load each user's orders = N+1 total
- Fix: Use Include() for eager loading

**Q3: How does change tracking work?**
- Load entity → EF creates snapshot
- Modify property → EF compares with snapshot
- SaveChanges → EF generates SQL for changed properties

**Q4: Tracking vs NoTracking?**
- Tracking: Auto-detect changes, more memory
- NoTracking: No change detection, for read-only, faster

**Q5: Include vs ThenInclude?**
- Include: Load one level
- ThenInclude: Load nested level

→ **See interview-qa.md for 25 more questions**

---

## Learning Path

**Day 1: Fundamentals**
- Read: why-entity-framework.md
- Read: dbcontext-fundamentals.md
- Understand: Why use EF, DbContext lifecycle

**Day 2: Core Features**
- Read: change-tracking.md
- Read: loading-strategies.md
- Focus: N+1 problem and prevention

**Day 3: Advanced**
- Read: query-optimization.md
- Read: patterns.md
- Study: Performance tuning

**Day 4: Production**
- Read: migrations.md
- Read: transactions-concurrency.md
- Understand: Schema versioning, ACID

**Day 5: Interview**
- Study: interview-qa.md
- Practice: Explaining concepts

---

## Common Mistakes to Avoid

❌ **DbContext as Singleton**
```csharp
// WRONG - Memory leaks, thread-safety issues
services.AddSingleton<EHRDbContext>();
```

✅ **DbContext as Scoped**
```csharp
// CORRECT
services.AddScoped<EHRDbContext>();
```

---

❌ **N+1 Queries**
```csharp
var patients = context.Patients.ToList();
foreach (var patient in patients)
{
    var count = patient.Appointments.Count; // Query per patient!
}
```

✅ **Eager Loading**
```csharp
var patients = context.Patients
    .Include(p => p.Appointments)
    .ToList();
```

---

❌ **Loading All Columns**
```csharp
var patients = context.Patients.ToList();
// Loads all 50 columns
```

✅ **Projection**
```csharp
var patients = context.Patients
    .Select(p => new { p.Id, p.Name })
    .ToList();
// Loads only 2 columns
```

---

## In the EHR Codebase

This project uses EF for:
- Patient, Appointment, MedicalRecord entities
- Change tracking and migrations
- Relationships between entities
- Standard CRUD operations

Dapper is used for complex reporting (see ../Dapper/).

---

## When to Choose EF vs Dapper

### Choose EF When:
- ✅ Standard CRUD operations
- ✅ Complex relationships
- ✅ Schema migrations needed
- ✅ New feature development
- ✅ Want type-safe queries

### Choose Dapper When:
- ✅ Complex reports (5+ table JOINs)
- ✅ Performance-critical reads
- ✅ Bulk operations (1M+ rows)
- ✅ Direct SQL optimization
- ✅ Stored procedures

→ **See ../orm-comparison.md for detailed comparison**

---

## Next Steps

1. **Read why-entity-framework.md** (understand benefits)
2. **Study dbcontext-fundamentals.md** (core concepts)
3. **Deep dive:** Pick what interests you most
   - Change tracking? → change-tracking.md
   - Performance? → query-optimization.md
   - Patterns? → patterns.md
4. **Interview prep:** interview-qa.md

---

## Quick Reference

### DbSet Operations
```csharp
// Read
var patient = await context.Patients.FirstAsync(p => p.Id == 1);

// Create
var newPatient = new Patient { Name = "Ahmed" };
await context.Patients.AddAsync(newPatient);

// Update
patient.Name = "New Name";
// No explicit Update() needed

// Delete
context.Patients.Remove(patient);

// Save
await context.SaveChangesAsync();
```

### Common LINQ
```csharp
// Filter
var active = context.Patients.Where(p => p.Active);

// Order
var sorted = context.Patients.OrderBy(p => p.Name);

// Pagination
var page = context.Patients
    .Skip((page - 1) * 10)
    .Take(10);

// Count
var total = context.Patients.Count();

// Any
var hasInactive = context.Patients.Any(p => !p.Active);

// Select
var names = context.Patients.Select(p => p.Name);
```

### Async Operations
```csharp
var patient = await context.Patients.FirstAsync();
var patients = await context.Patients.ToListAsync();
var count = await context.Patients.CountAsync();
var any = await context.Patients.AnyAsync(p => p.Active);
```

---

## Related Docs

- **ORM Overview:** ../README.md
- **Dapper Guide:** ../Dapper/README.md
- **ORM Comparison:** ../orm-comparison.md
- **Hybrid Integration:** ../Hybrid/ef-dapper-integration.md
- **EHR Examples:** ../Hybrid/ehr-practical-examples.md
