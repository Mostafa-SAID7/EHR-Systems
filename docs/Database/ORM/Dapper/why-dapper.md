# Why Use Dapper?

## Dapper: Micro-ORM Philosophy

**"Micro-ORM"** = Lightweight wrapper around ADO.NET that maps SQL results to objects.

```csharp
// Write SQL, get objects back
var patients = await connection.QueryAsync<Patient>(
    "SELECT * FROM Patients WHERE Status = @Status",
    new { Status = "Active" }
);
```

---

## Advantages ✅

### 1. Performance - Minimal Overhead

```
Dapper:     ~5ms (per 100 queries)
EF Core:    ~35ms (with AsNoTracking)
EF Core:    ~50ms (with tracking)
Raw ADO:    ~3ms (direct speed)

Dapper: 95% of raw ADO speed, 99% of EF speed
```

**Why?**
- No change tracking
- No query translation layer
- Direct parameter mapping
- Minimal object creation overhead

**Real-world:** Dashboard query
```csharp
// EF: 150ms (complex LINQ translation)
var stats = await context.Patients
    .GroupBy(p => p.Status)
    .Select(g => new { Status = g.Key, Count = g.Count() })
    .ToListAsync();

// Dapper: 15ms (direct SQL)
var stats = await connection.QueryAsync<StatusStats>(
    "SELECT Status, COUNT(*) as Count FROM Patients GROUP BY Status"
);
```

### 2. Simplicity - Tiny API

```csharp
// That's it. Four methods cover 90% of use cases:

// Query (SELECT) → Multiple objects
await connection.QueryAsync<T>(sql, param);

// QueryFirstOrDefault (SELECT) → Single object or null
await connection.QueryFirstOrDefaultAsync<T>(sql, param);

// Execute (INSERT/UPDATE/DELETE)
await connection.ExecuteAsync(sql, param);

// ExecuteScalar (SELECT COUNT/SUM/etc)
await connection.ExecuteScalarAsync<T>(sql, param);
```

**Compare to EF Core:**
- DbContext
- DbSet
- Change tracker
- Migrations
- Navigation properties
- Lazy loading
- Relationships
- Concurrency tokens
- Query filters
- Value converters
... and 100+ other concepts

### 3. Control - Direct SQL

```csharp
// ✅ You write the exact SQL
var patients = await connection.QueryAsync<Patient>(
    @"SELECT Id, Name, MRN
      FROM Patients
      WHERE CreatedAt >= @StartDate
        AND CreatedAt < @EndDate
      ORDER BY CreatedAt DESC"
);

// You know exactly what query runs
// You can optimize it directly
// No ORM guessing about your intent
```

### 4. Flexibility - Any SQL Pattern

```csharp
// Stored procedures
await connection.QueryAsync<Report>(
    "sp_GenerateReport",
    commandType: CommandType.StoredProcedure
);

// Complex JOINs
var results = await connection.QueryAsync(
    @"SELECT *
      FROM Patients p
      JOIN Appointments a ON p.Id = a.PatientId
      JOIN Doctors d ON a.DoctorId = d.Id
      WHERE p.Status = @Status"
);

// Window functions
var ranked = await connection.QueryAsync(
    @"SELECT *,
             ROW_NUMBER() OVER (ORDER BY CreatedAt) as RowNum
      FROM Patients"
);

// CTEs (Common Table Expressions)
var results = await connection.QueryAsync(
    @"WITH PatientStats AS (
        SELECT PatientId, COUNT(*) as AppointmentCount
        FROM Appointments
        GROUP BY PatientId
      )
      SELECT p.*, ps.AppointmentCount
      FROM Patients p
      JOIN PatientStats ps ON p.Id = ps.PatientId"
);

// All supported - no ORM getting in the way
```

### 5. Bulk Operations - Lightning Fast

```csharp
// Insert 1,000,000 rows in seconds

var rows = GenerateMillionPatients();

// ✅ Dapper - Direct execute, no tracking
var inserted = await connection.ExecuteAsync(
    @"INSERT INTO Patients (Name, Email, MRN)
      VALUES (@Name, @Email, @MRN)",
    rows  // Entire collection at once
);
// Time: ~500ms for 1M rows

// ❌ EF Core - Change tracking nightmare
context.Patients.AddRange(rows); // Tracks all 1M
await context.SaveChangesAsync();
// Time: ~5000ms for 1M rows (10x slower!)
```

### 6. Parameterization - SQL Injection Prevention

```csharp
// ✅ Safe - Parameters
var patient = await connection.QueryFirstOrDefaultAsync<Patient>(
    "SELECT * FROM Patients WHERE Email = @Email",
    new { Email = userInput }
);
// Safe even if userInput = "'; DROP TABLE Patients; --"

// ❌ Dangerous - String concatenation
var patient = await connection.QueryFirstOrDefaultAsync<Patient>(
    $"SELECT * FROM Patients WHERE Email = '{userInput}'"
);
// SQL Injection vulnerability!
```

---

## Trade-offs ❌

### 1. No Change Tracking

```csharp
var patient = await connection.QueryFirstOrDefaultAsync<Patient>(
    "SELECT * FROM Patients WHERE Id = @Id",
    new { Id = 1 }
);

patient.Name = "New Name";

// Dapper doesn't know it changed
// You must manually write UPDATE
await connection.ExecuteAsync(
    "UPDATE Patients SET Name = @Name WHERE Id = @Id",
    patient
);
```

**Solution:** For simple CRUD, use EF. For reports, Dapper doesn't need change tracking.

### 2. No Migrations

```csharp
// EF automatic
dotnet ef migrations add AddPhoneColumn

// Dapper - manual SQL scripts
/*
CREATE MIGRATION: 2024_01_15_AddPhoneColumn.sql
ALTER TABLE Patients ADD PhoneNumber NVARCHAR(20);
*/
```

**Solution:** Use EF for schema management, Dapper for queries.

### 3. No Relationship Navigation

```csharp
// ❌ Doesn't work in Dapper
var patient = await connection.QueryFirstOrDefaultAsync<Patient>(
    "SELECT * FROM Patients WHERE Id = @Id",
    new { Id = 1 }
);
Console.WriteLine(patient.Appointments.Count); // Empty!

// ✅ Must manually JOIN and map
var sql = @"
    SELECT p.Id, p.Name, a.Id, a.AppointmentDate
    FROM Patients p
    LEFT JOIN Appointments a ON p.Id = a.PatientId";

var patients = await connection.QueryAsync<Patient, Appointment, Patient>(
    sql,
    (patient, appointment) =>
    {
        patient.Appointments.Add(appointment);
        return patient;
    },
    splitOn: "Id"
);
```

**Solution:** For related data, use EF. For reports, write explicit JOINs.

### 4. Manual Type Mapping

```csharp
// EF converts automatically
var patient = await context.Patients.FirstAsync();

// Dapper requires exact column match
var patient = await connection.QueryFirstOrDefaultAsync<Patient>(
    "SELECT Id, Name, MRN FROM Patients"  // Columns match Patient properties
);

// Or use custom mapping
connection.QueryAsync<dynamic>(sql); // Returns dynamic, not typed
```

---

## When to Use Dapper

### ✅ Perfect Use Cases

**1. Complex Reports**
```csharp
// 5+ table JOINs, aggregations
var report = await dapperContext.QueryAsync<ReportRow>(
    @"SELECT p.Name, COUNT(a.Id) as ApptCount, SUM(i.Amount) as Total
      FROM Patients p
      LEFT JOIN Appointments a ON p.Id = a.PatientId
      LEFT JOIN Invoices i ON p.Id = i.PatientId
      WHERE p.Status = @Status
      GROUP BY p.Id, p.Name"
);
```

**2. Performance-Critical Reads**
```csharp
// Dashboard, real-time stats
var stats = await dapperContext.QueryFirstOrDefaultAsync<DashboardStats>(
    "SELECT TOP 1 * FROM vw_DashboardStats WHERE Date = @Date",
    new { Date = DateTime.Today }
);
// Must be fast, needs to run every 5 seconds
```

**3. Bulk Operations**
```csharp
// Insert/update 1M+ rows
var rowsAffected = await dapperContext.ExecuteAsync(
    "INSERT INTO PatientAudit SELECT * FROM Patients WHERE CreatedAt < @Date",
    new { Date = DateTime.Now.AddYears(-1) }
);
```

**4. Stored Procedures**
```csharp
// Legacy SP, or complex business logic in database
var results = await dapperContext.QueryAsync<PatientAudit>(
    "sp_GenerateComplianceReport",
    new { StartDate = startDate, EndDate = endDate },
    commandType: CommandType.StoredProcedure
);
```

**5. Database-Specific Features**
```csharp
// Window functions, CTEs, etc not easily expressible in LINQ
var ranked = await dapperContext.QueryAsync<RankedPatient>(
    @"SELECT *,
             ROW_NUMBER() OVER (ORDER BY CreatedAt DESC) as Rank
      FROM Patients
      WHERE Status = @Status"
);
```

### ❌ NOT for These

**DON'T use Dapper for:**
- Standard CRUD (use EF)
- Complex relationships (use EF)
- Schema changes (use EF migrations)
- When you don't know SQL (use EF LINQ)

---

## Hybrid Approach (This EHR)

**Best practice: Use both EF and Dapper**

```csharp
// EF for everything
public async Task CreatePatientAsync(CreatePatientDto dto)
{
    var patient = new Patient { Name = dto.Name, MRN = dto.MRN };
    context.Patients.Add(patient);
    await context.SaveChangesAsync();
}

// Dapper only for complex queries
public async Task<DashboardData> GetDashboardAsync()
{
    return await dapperContext.QueryFirstOrDefaultAsync<DashboardData>(
        @"SELECT 
            COUNT(DISTINCT p.Id) as PatientCount,
            COUNT(DISTINCT a.Id) as AppointmentCount,
            SUM(i.Amount) as TotalBilled
          FROM Patients p
          LEFT JOIN Appointments a ON p.Id = a.PatientId
          LEFT JOIN Invoices i ON p.Id = i.PatientId
          WHERE p.Status = @Status",
        new { Status = "Active" }
    );
}
```

**Why both?**
- EF: Handles CRUD, relationships, migrations - 80% of code
- Dapper: Handles reporting, performance, bulk ops - 20% of code
- Together: 100% productivity without compromises

---

## Interview Questions About Dapper

**Q: Why use Dapper over Entity Framework?**

A: For performance-critical queries, complex reports with many JOINs, bulk operations, or when you need direct SQL control without ORM overhead.

**Q: What are Dapper's limitations?**

A: No change tracking, no migrations, no relationship navigation, manual type mapping. Use EF for CRUD, Dapper for reports.

**Q: How to prevent SQL Injection in Dapper?**

A: Always use parameterized queries with @ParameterName syntax. Pass parameters as objects, never concatenate strings.

**Q: Can EF and Dapper work together?**

A: Yes! Dapper can reuse EF's DbContext connection, so they can share the same database session.

**Q: Performance vs EF Core?**

A: Dapper is 10-20% faster on simple queries, but 5-10x faster on complex reports because no query translation layer.

---

## When to Learn Dapper

1. **After EF Core basics** - Know CRUD first
2. **When you hit performance walls** - Reporting becomes slow
3. **Before complex queries** - Learn for dashboard, analytics
4. **For legacy systems** - Stored procedures are common

**Priority:** Medium
- Learn EF first (more common)
- Then learn Dapper (for real-world performance)
- You'll use both in production

---

## Key Takeaway

**Dapper is not a replacement for Entity Framework.**

It's a **complementary tool** for when EF isn't fast enough or when direct SQL control is needed.

**Philosophy:**
- EF for 80% of queries (CRUD, standard operations)
- Dapper for 20% of queries (reports, performance, bulk)
- Result: Best productivity + best performance
