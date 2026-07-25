# Dapper - Lightweight ORM Guide

## What is Dapper?

Dapper is a **micro-ORM** - lightweight, ultra-fast object mapper for SQL.

```csharp
// Simple, direct SQL
var patient = await connection.QueryFirstOrDefaultAsync<Patient>(
    "SELECT Id, Name, MRN FROM Patients WHERE Id = @Id",
    new { Id = 1 }
);
```

### Key Philosophy
**"Stay close to the metal"** - Write SQL, let Dapper map to objects.

---

## Why Dapper?

### Advantages ✅
- **Performance** - Fastest micro-ORM (minimal overhead)
- **Control** - Write exact SQL you want
- **Simplicity** - Tiny API, easy to learn
- **Flexibility** - Handle any SQL pattern
- **Bulk operations** - Efficient for large datasets
- **Complex queries** - No ORM overhead

### Trade-offs ❌
- **No change tracking** - Manual update tracking
- **No migrations** - Schema changes manual
- **No relationships** - Manual JOIN handling
- **SQL required** - Must write SQL strings
- **Type safety** - Less than EF

---

## Folder Contents

| File | Focus |
|------|-------|
| **why-dapper.md** | Dapper benefits, when to use |
| **dapper-fundamentals.md** | Basic queries, parameter mapping |
| **query-patterns.md** | SELECT, INSERT, UPDATE, DELETE patterns |
| **advanced-features.md** | Multi-mapping, stored procedures, bulk ops |
| **performance-tuning.md** | Query optimization, caching, benchmarking |
| **interview-qa.md** | Interview questions specific to Dapper |

---

## Quick Start (5 minutes)

### 1. Install Dapper
```bash
dotnet add package Dapper
```

### 2. Get Connection
```csharp
var connection = new SqlConnection("connection-string");
```

### 3. Query
```csharp
// Single object
var patient = await connection.QueryFirstOrDefaultAsync<Patient>(
    "SELECT * FROM Patients WHERE Id = @Id",
    new { Id = 1 }
);

// Multiple objects
var patients = await connection.QueryAsync<Patient>(
    "SELECT * FROM Patients WHERE Active = @Active",
    new { Active = true }
);

// Scalar
var count = await connection.ExecuteScalarAsync<int>(
    "SELECT COUNT(*) FROM Patients"
);
```

### 4. Execute (INSERT/UPDATE/DELETE)
```csharp
var rowsAffected = await connection.ExecuteAsync(
    @"UPDATE Patients SET Name = @Name, UpdatedAt = @Now 
      WHERE Id = @Id",
    new { Name = "Ahmed", Now = DateTime.UtcNow, Id = 1 }
);
```

---

## Core Concepts

### Simple Query Pattern
```csharp
// SQL → Objects
var sql = "SELECT Id, Name, MRN FROM Patients WHERE Status = @Status";
var parameters = new { Status = "Active" };
var patients = await connection.QueryAsync<Patient>(sql, parameters);

// Dapper maps columns to properties:
// Id → Patient.Id
// Name → Patient.Name
// MRN → Patient.MRN
```

### Parameterized Queries (Prevent SQL Injection)
```csharp
// ✅ SAFE - Parameterized
var sql = "SELECT * FROM Patients WHERE Email = @Email";
var patient = await connection.QueryFirstOrDefaultAsync<Patient>(
    sql, 
    new { Email = "user@example.com" }
);

// ❌ DANGEROUS - String concatenation
var sql = "SELECT * FROM Patients WHERE Email = '" + email + "'";
// SQL Injection vulnerability!
```

### Multi-Mapping (JOINs)
```csharp
// Query returns both Patient and Appointment data
var sql = @"
    SELECT p.Id, p.Name, a.Id, a.AppointmentDate
    FROM Patients p
    JOIN Appointments a ON p.Id = a.PatientId
    WHERE p.Id = @Id";

var patient = await connection.QueryAsync<Patient, Appointment, Patient>(
    sql,
    (p, a) => { p.Appointments.Add(a); return p; },
    new { Id = 1 },
    splitOn: "Id"
);
```

---

## Most Critical Interview Questions

**Q1: When to use Dapper over Entity Framework?**
- Complex reports (5+ table JOINs)
- Performance-critical reads
- Bulk operations
- Direct SQL needed

**Q2: How to prevent SQL Injection with Dapper?**
- Always use parameterized queries
- @ParameterName syntax
- Pass parameters object

```csharp
// ✅ Safe
var sql = "SELECT * FROM Patients WHERE Id = @Id";
await connection.QueryAsync<Patient>(sql, new { Id = 1 });

// ❌ Unsafe
var sql = $"SELECT * FROM Patients WHERE Id = {id}";
```

**Q3: Multi-mapping - what is it?**
- Mapping query result to multiple object types
- Useful for JOINs across tables
- Dapper combines results into parent object

**Q4: Performance advantages over EF?**
- No change tracking overhead
- Direct SQL execution
- Minimal object creation
- No query translation layer

**Q5: How to execute stored procedure?**
```csharp
var result = await connection.QueryAsync<Patient>(
    "sp_GetPatients",
    commandType: CommandType.StoredProcedure
);
```

→ **See interview-qa.md for 15 more questions**

---

## In the EHR Codebase

### DapperContext Wrapper
```csharp
public class DapperContext : IDapperContext
{
    private readonly DbContext _dbContext;
    
    // Reuses EF's connection
    private async Task<IDbConnection> GetOpenConnectionAsync()
    {
        var conn = _dbContext.Database.GetDbConnection();
        if (conn.State != ConnectionState.Open)
            await _dbContext.Database.OpenConnectionAsync();
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

### Usage Pattern
```csharp
public class AnalyticsService
{
    private readonly IDapperContext _dapper;
    
    public async Task<DashboardData> GetDashboardAsync()
    {
        // Complex query - perfect for Dapper
        return await _dapper.QueryFirstOrDefaultAsync<DashboardData>(
            @"SELECT 
                COUNT(DISTINCT p.Id) as TotalPatients,
                COUNT(DISTINCT a.Id) as TotalAppointments,
                SUM(i.Amount) as TotalBilled
              FROM Patients p
              LEFT JOIN Appointments a ON p.Id = a.PatientId
              LEFT JOIN Invoices i ON p.Id = i.PatientId
              WHERE p.Status = @Status",
            new { Status = "Active" }
        );
    }
}
```

---

## Common Patterns

### Pattern 1: Query with Parameters
```csharp
var sql = @"
    SELECT Id, Name, Email, Status
    FROM Patients
    WHERE Active = @Active
      AND CreatedAt >= @StartDate
      AND CreatedAt < @EndDate";

var patients = await connection.QueryAsync<Patient>(
    sql,
    new 
    { 
        Active = true, 
        StartDate = new DateTime(2024, 1, 1),
        EndDate = new DateTime(2024, 12, 31)
    }
);
```

### Pattern 2: INSERT and get ID
```csharp
var sql = @"
    INSERT INTO Patients (Name, Email, MRN)
    VALUES (@Name, @Email, @MRN);
    SELECT CAST(SCOPE_IDENTITY() as int);";

var newPatientId = await connection.QuerySingleAsync<int>(
    sql,
    new { Name = "Ahmed", Email = "ahmed@test.com", MRN = "123456" }
);
```

### Pattern 3: Bulk INSERT
```csharp
var sql = @"
    INSERT INTO Patients (Name, Email, MRN, CreatedAt)
    VALUES (@Name, @Email, @MRN, @CreatedAt)";

// Batch insert all at once
var rowsInserted = await connection.ExecuteAsync(
    sql,
    patients.Select(p => new 
    { 
        p.Name, 
        p.Email, 
        p.MRN, 
        CreatedAt = DateTime.UtcNow 
    })
);
```

---

## When to Use Dapper vs EF

### Use Dapper When:
- ✅ Complex reporting with many JOINs
- ✅ Performance is critical
- ✅ Bulk operations (INSERT 1M+ rows)
- ✅ Stored procedures
- ✅ Complex aggregations
- ✅ Raw SQL is simpler

### Use EF When:
- ✅ Standard CRUD
- ✅ Complex relationships
- ✅ Schema migrations
- ✅ Change tracking needed
- ✅ Want type-safe queries

→ **See ../orm-comparison.md for detailed comparison**

---

## Learning Path

**Day 1: Basics**
- Read: why-dapper.md
- Read: dapper-fundamentals.md
- Write: Simple SELECT queries

**Day 2: Patterns**
- Read: query-patterns.md
- Practice: INSERT, UPDATE, DELETE
- Understand: Parameterization

**Day 3: Advanced**
- Read: advanced-features.md
- Study: Multi-mapping, stored procedures
- Learn: Bulk operations

**Day 4: Performance**
- Read: performance-tuning.md
- Benchmark: Your queries
- Optimize: Slow queries

**Day 5: Interview**
- Study: interview-qa.md
- Understand: When to use Dapper

---

## Quick Reference

### Query Methods
```csharp
// Single or null
var patient = await conn.QueryFirstOrDefaultAsync<Patient>(sql, param);

// Single (throws if not found)
var patient = await conn.QuerySingleAsync<Patient>(sql, param);

// Multiple
var patients = await conn.QueryAsync<Patient>(sql, param);

// Scalar (single value)
var count = await conn.ExecuteScalarAsync<int>(sql, param);

// Non-query
var rowsAffected = await conn.ExecuteAsync(sql, param);
```

### Command Types
```csharp
// SQL
var patients = await conn.QueryAsync<Patient>(sql);

// Stored procedure
var patients = await conn.QueryAsync<Patient>(
    "sp_GetPatients",
    commandType: CommandType.StoredProcedure
);
```

---

## Related Docs

- **ORM Overview:** ../README.md
- **Entity Framework:** ../EntityFramework/README.md
- **ORM Comparison:** ../orm-comparison.md
- **Hybrid Integration:** ../Hybrid/ef-dapper-integration.md
- **EHR Examples:** ../Hybrid/ehr-practical-examples.md
