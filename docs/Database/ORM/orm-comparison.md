# ORM Comparison: Entity Framework vs Dapper vs Raw SQL

## Overview Table

| Aspect | Entity Framework | Dapper | Raw SQL |
|--------|-----------------|--------|---------|
| **Performance** | Good | Excellent | Excellent |
| **Learning Curve** | Steep | Easy | Easy |
| **Type Safety** | ✅ Strong | ⚠️ Partial | ❌ None |
| **Code Size** | Minimal | Small | Large |
| **Change Tracking** | ✅ Automatic | ❌ Manual | ❌ None |
| **Relationships** | ✅ Built-in | ❌ Manual | ❌ None |
| **Migrations** | ✅ Automatic | ❌ Manual | ❌ Manual |
| **LINQ Queries** | ✅ Yes | ❌ No | ❌ No |
| **Async Support** | ✅ Excellent | ✅ Excellent | ⚠️ Limited |
| **Flexibility** | Good | Excellent | Excellent |
| **Community** | ✅ Large | Good | N/A |
| **Documentation** | ✅ Excellent | Good | Good |

---

## Feature Comparison

### 1. Type Safety & IntelliSense

#### Entity Framework
```csharp
// ✅ Type-safe at compile-time
var patients = await context.Patients
    .Where(p => p.Status == "Active")  // IntelliSense
    .OrderBy(p => p.Name)
    .ToListAsync();
// Typo? Compile error!
```

#### Dapper
```csharp
// ⚠️ Type-safe at property level only
var patients = await connection.QueryAsync<Patient>(
    "SELECT * FROM Patients WHERE Status = @Status",
    new { Status = "Active" }  // No IntelliSense on SQL
);
// Typo in column name? Runtime error!
```

#### Raw SQL
```csharp
// ❌ No type safety
var patients = database.ExecuteQuery(
    "SELECT * FROM Patients WHERE Status = 'Active'"  // String literal
);
```

---

### 2. Change Tracking

#### Entity Framework
```csharp
var patient = context.Patients.First(p => p.Id == 1);
patient.Name = "New Name";
// EF detects change automatically

await context.SaveChangesAsync();
// EF generates: UPDATE Patients SET Name = 'New Name' WHERE Id = 1
```

#### Dapper
```csharp
var patient = new Patient { Id = 1, Name = "New Name" };

// You must write the UPDATE
var rowsAffected = await connection.ExecuteAsync(
    "UPDATE Patients SET Name = @Name WHERE Id = @Id",
    patient
);
```

#### Raw SQL
```csharp
// Complete manual control
database.ExecuteNonQuery(
    "UPDATE Patients SET Name = 'New Name' WHERE Id = 1"
);
```

---

### 3. Relationships

#### Entity Framework
```csharp
// ✅ Relationships built-in
public class Patient
{
    public int Id { get; set; }
    public ICollection<Appointment> Appointments { get; set; }
}

var patient = await context.Patients
    .Include(p => p.Appointments)
    .FirstAsync();

Console.WriteLine(patient.Appointments.Count); // ✅ Works
```

#### Dapper
```csharp
// ❌ No relationship support, manual JOIN
var sql = @"
    SELECT p.Id, p.Name, a.Id, a.AppointmentDate
    FROM Patients p
    LEFT JOIN Appointments a ON p.Id = a.PatientId
    WHERE p.Id = @Id";

var result = await connection.QueryAsync<Patient, Appointment, Patient>(
    sql,
    (p, a) => { p.Appointments.Add(a); return p; },
    splitOn: "Id"
);
```

#### Raw SQL
```csharp
// Complete manual control
var rows = database.ExecuteQuery(
    "SELECT * FROM Patients p LEFT JOIN Appointments a ON..."
);
// Parse results manually
```

---

### 4. Migrations

#### Entity Framework
```csharp
// ✅ Automatic schema versioning
public class Patient
{
    public int Id { get; set; }
    public string Phone { get; set; } // New field
}

// Then:
dotnet ef migrations add AddPhoneColumn
dotnet ef database update
// Automatically handles schema change
```

#### Dapper
```csharp
// ❌ No automatic migrations
// Manual SQL scripts required:
// ALTER TABLE Patients ADD Phone NVARCHAR(20);
// Create migration files manually
```

#### Raw SQL
```csharp
// ❌ Manual DDL
database.ExecuteNonQuery(
    "ALTER TABLE Patients ADD Phone NVARCHAR(20)"
);
// Track changes manually
```

---

### 5. Query Complexity

#### Entity Framework
```csharp
// ✅ Simple queries are very simple
var activePatients = context.Patients
    .Where(p => p.Active)
    .ToListAsync();
```

#### Dapper
```csharp
// Still simple, but requires SQL
var activePatients = await connection.QueryAsync<Patient>(
    "SELECT * FROM Patients WHERE Active = 1"
);
```

#### Raw SQL
```csharp
// Manual everything
var patients = database.ExecuteQuery("SELECT * FROM Patients WHERE Active = 1");
```

---

## Decision Tree: Which ORM to Use?

```
Question: What are you doing?
│
├─ Simple CRUD (Create, Read, Update, Delete)?
│  └─ Use: Entity Framework
│     Why: Auto-tracking, minimal code, relationships
│
├─ Complex report (5+ table JOINs, aggregations)?
│  └─ Use: Dapper
│     Why: Write exact SQL, best performance, clear logic
│
├─ Bulk operation (INSERT/UPDATE 1M+ rows)?
│  └─ Use: Dapper
│     Why: Direct SQL, no change tracking overhead
│
├─ Stored procedure call?
│  └─ Use: Dapper
│     Why: Built-in support, simple execution
│
├─ Real-time analytics, complex aggregations?
│  └─ Use: Dapper or Raw SQL
│     Why: Performance critical, optimized SQL
│
└─ Need database-specific feature?
   └─ Use: Raw SQL
      Why: Full control, specific DB feature
```

---

## Performance Comparison

### Simple Query (100 executions)

```
Entity Framework:  ~50ms  (with tracking)
Entity Framework:  ~35ms  (with AsNoTracking)
Dapper:            ~5ms   (minimal overhead)
Raw ADO.NET:       ~3ms   (direct execution)
```

**Verdict:** For simple queries, difference is negligible (35ms vs 5ms per 100 queries).

### Complex Report (1 execution)

```
Entity Framework:  Slow (complex LINQ translation)
Dapper:            Fast (direct SQL)
Raw SQL:           Fast (direct SQL)
```

**Verdict:** Dapper shines on complex queries.

### Bulk Insert (100,000 rows)

```
EF AddRange:       ~2000ms + tracking overhead
Dapper Execute:    ~500ms  (no overhead)
Raw ADO Batch:     ~400ms  (raw speed)
```

**Verdict:** Dapper wins for bulk operations.

---

## Code Size Comparison

### Same Task: Get patient with appointments

#### Entity Framework (6 lines)
```csharp
var patient = await context.Patients
    .Include(p => p.Appointments)
    .FirstAsync(p => p.Id == id);
// That's it!
```

#### Dapper (10 lines)
```csharp
const string sql = @"
    SELECT p.*, a.*
    FROM Patients p
    LEFT JOIN Appointments a ON p.Id = a.PatientId
    WHERE p.Id = @Id";
    
var patient = await connection.QueryAsync<Patient, Appointment, Patient>(
    sql, (p, a) => { p.Appointments.Add(a); return p; },
    new { Id = id }, splitOn: "Id"
);
```

#### Raw SQL (20+ lines)
```csharp
var cmd = new SqlCommand(
    @"SELECT p.*, a.* FROM Patients p 
      LEFT JOIN Appointments a ON p.Id = a.PatientId 
      WHERE p.Id = @Id", connection);
cmd.Parameters.AddWithValue("@Id", id);
var reader = await cmd.ExecuteReaderAsync();

var patient = new Patient();
var appointments = new List<Appointment>();

while (await reader.ReadAsync())
{
    if (patient.Id == 0)
    {
        patient.Id = reader.GetInt32(0);
        patient.Name = reader.GetString(1);
    }
    
    var appt = new Appointment { Id = reader.GetInt32(...) };
    appointments.Add(appt);
}
patient.Appointments = appointments;
```

**Verdict:** EF most concise, Dapper middle, Raw SQL verbose.

---

## When Each Excel

### Entity Framework Excels At:
1. **Rapid development** - Less code per feature
2. **Complex relationships** - Multiple levels of includes
3. **CRUD operations** - Create, Read, Update, Delete
4. **Schema evolution** - Migrations built-in
5. **Standard business logic** - Normal queries

**Best for:** New features, MVP, business logic layer

---

### Dapper Excels At:
1. **Performance-critical reads** - Reports, dashboards
2. **Complex SQL** - 5+ table JOINs, complex aggregations
3. **Bulk operations** - 1M+ row inserts/updates
4. **Direct SQL control** - Optimization opportunities
5. **Stored procedures** - Legacy DB support

**Best for:** Reports, analytics, performance-tuned queries, legacy systems

---

### Raw SQL Excels At:
1. **Database-specific features** - Window functions, CTEs
2. **Extreme performance** - Hand-tuned queries
3. **Complex dynamic queries** - Runtime SQL building
4. **Legacy systems** - Procedural approach
5. **One-off queries** - Maintenance scripts

**Best for:** Advanced scenarios, legacy support, extreme optimization

---

## Hybrid Approach (This EHR System)

The EHR uses **both EF and Dapper**:

```csharp
// Entity Framework for writes
public async Task CreatePatientAsync(Patient patient)
{
    context.Patients.Add(patient);
    await context.SaveChangesAsync();
}

// Dapper for complex reports
public async Task<DashboardData> GetDashboardAsync()
{
    return await dapperContext.QueryFirstOrDefaultAsync<DashboardData>(
        @"SELECT COUNT(*) as PatientCount,
                 SUM(Amount) as TotalBilled
          FROM Patients p LEFT JOIN Invoices i ON p.Id = i.PatientId"
    );
}
```

### Why Both?
- **EF:** Relationships, change tracking, migrations, standard CRUD
- **Dapper:** Performance-critical reads, complex reports, bulk ops
- **Together:** Best of both worlds

---

## Migration Strategy

### If Starting Fresh:
1. Start with **Entity Framework**
2. Use Dapper only when performance bottlenecks appear
3. 80/20 rule: EF for 80% of code, Dapper for 20% performance-critical

### If Have Legacy System:
1. Start with **Dapper** (less intrusive)
2. Gradually migrate to EF where applicable
3. Keep Dapper for complex reports

---

## Interview Questions

**Q: When would you use Dapper over Entity Framework?**
A: For performance-critical queries, complex reports with many JOINs, bulk operations, or when you need direct SQL control.

**Q: Why not just use raw SQL for everything?**
A: Raw SQL is verbose, error-prone, and requires manual mapping. EF automates this for standard queries.

**Q: Can you use EF and Dapper together?**
A: Yes! EF for CRUD, Dapper for reports. They can share the same DbContext connection.

**Q: What's the performance difference?**
A: For simple queries: negligible. For complex reports: Dapper 10x faster. For bulk ops: Dapper 5x faster.

**Q: DbContext overhead - is it real?**
A: Change tracking has overhead (memory, CPU). Use AsNoTracking() for read-only to eliminate it.

---

## Recommendation by Scenario

| Scenario | Use | Reason |
|----------|-----|--------|
| New feature | EF | Fast development, relationships |
| Patient report | Dapper | Complex query, performance |
| Invoice list | EF | Standard CRUD |
| Dashboard stats | Dapper | Aggregations, performance |
| Create appointment | EF | Change tracking, validation |
| Bulk patient import | Dapper | Performance, bulk insert |
| Search by criteria | EF | LINQ, flexibility |
| Analytics query | Dapper | Complex SQL, optimization |

---

## Conclusion

**For most applications:**
- 80% Entity Framework (standard operations)
- 20% Dapper (performance-critical queries)
- <1% Raw SQL (database-specific features)

Start with EF, add Dapper when you hit performance walls. Don't over-optimize prematurely.

---

## Next Steps

1. **Learn Entity Framework:** EntityFramework/README.md
2. **Learn Dapper:** Dapper/README.md
3. **See practical examples:** Hybrid/ehr-practical-examples.md
4. **Understand integration:** Hybrid/ef-dapper-integration.md
