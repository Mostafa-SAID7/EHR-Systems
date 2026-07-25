# Raw SQL - Direct Database Access

## What is Raw SQL?

Raw SQL means writing SQL directly using ADO.NET without any ORM layer.

```csharp
// Raw SQL with SqlCommand
using var connection = new SqlConnection("connection-string");
using var command = new SqlCommand(
    "SELECT * FROM Patients WHERE Id = @Id",
    connection
);
command.Parameters.AddWithValue("@Id", 1);

using var reader = await command.ExecuteReaderAsync();
while (await reader.ReadAsync())
{
    var patientId = reader.GetInt32(0);
    var name = reader.GetString(1);
}
```

---

## Why Use Raw SQL?

### Advantages ✅

**1. Maximum Performance**
```csharp
// Fastest possible database access
// No translation layer, no mapping overhead
// Direct execution
```

**2. Full Control**
```csharp
// Write exactly what you need
// No ORM assumptions or limitations
// Database-specific features
```

**3. Simple for One-Offs**
```csharp
// Quick queries, no setup needed
// Minimal dependencies
// No framework knowledge required
```

**4. Legacy System Support**
```csharp
// Works with any database
// No schema requirements
// Procedural/stored procedure friendly
```

### Trade-offs ❌

**1. Verbose**
```csharp
// Much more boilerplate code
// Manual parameter handling
// Manual result mapping
```

**2. No Type Safety**
```csharp
// Strings everywhere
// Column names as magic strings
// Runtime errors for typos
```

**3. Manual Everything**
```csharp
// Manual connection management
// Manual transaction handling
// Manual parameter escaping
```

**4. No Migrations**
```csharp
// Schema changes manual
// No version control for schema
// Hard to track changes
```

---

## When to Use Raw SQL

### ✅ Perfect Use Cases

**1. Database Maintenance Scripts**
```sql
-- One-time data fixes, cleanup scripts
UPDATE Patients SET Status = 'Inactive' WHERE LastVisit < DATEADD(YEAR, -1, GETDATE());
DELETE FROM TempImport WHERE Status = 'Failed';
```

**2. Extreme Performance Requirements**
```csharp
// Ultra-fast data export
// Millions of rows, milliseconds matter
var sw = Stopwatch.StartNew();
using var reader = command.ExecuteReader();
// Process directly without mapping
sw.Stop(); // Should be < 100ms
```

**3. Complex Database Features**
```sql
-- Window functions, CTEs, advanced SQL Server features
WITH PatientStats AS (
    SELECT PatientId, COUNT(*) as AppointmentCount,
           ROW_NUMBER() OVER (ORDER BY COUNT(*) DESC) as Rank
    FROM Appointments
    GROUP BY PatientId
)
SELECT * FROM PatientStats WHERE Rank <= 10;
```

**4. Stored Procedures Directly**
```csharp
// Legacy SP with complex logic
command.CommandType = CommandType.StoredProcedure;
command.CommandText = "sp_GenerateComplianceReport";
```

**5. Dynamic SQL**
```csharp
// Runtime query building (with caution!)
string sql = "SELECT * FROM Patients WHERE 1=1";
if (!string.IsNullOrEmpty(searchTerm))
    sql += " AND Name LIKE @SearchTerm";
// Build dynamically
```

### ❌ NOT for These

- Standard CRUD (use EF)
- Complex relationships (use EF)
- Bulk operations (use Dapper)
- When you don't need raw SQL performance
- Anything in production without escaping

---

## Folder Contents

| File | Focus |
|------|-------|
| **README.md** | This file - overview |
| **why-raw-sql.md** | When to use, advantages/disadvantages |
| **raw-sql-fundamentals.md** | SqlCommand, parameters, connection management |
| **query-patterns.md** | SELECT, INSERT, UPDATE, DELETE, stored procedures |
| **advanced-features.md** | Dynamic SQL, transactions, bulk operations |
| **security-best-practices.md** | SQL injection prevention, parameterization |
| **interview-qa.md** | Interview questions about raw SQL |

---

## Quick Start

### 1. Create Connection
```csharp
var connection = new SqlConnection("connection-string");
```

### 2. Create Command
```csharp
var command = new SqlCommand("SELECT * FROM Patients", connection);
```

### 3. Add Parameters
```csharp
command.Parameters.AddWithValue("@Id", 1);
```

### 4. Execute and Read
```csharp
await connection.OpenAsync();
using var reader = await command.ExecuteReaderAsync();
while (await reader.ReadAsync())
{
    var id = reader.GetInt32(0);
    var name = reader.GetString(1);
}
```

---

## Core Methods

### SqlCommand Methods

```csharp
// SELECT → DataReader (streaming)
using var reader = command.ExecuteReader();

// INSERT/UPDATE/DELETE → Rows Affected
int rowsAffected = command.ExecuteNonQuery();

// Scalar (COUNT, MAX, etc) → Single Value
int count = (int)command.ExecuteScalar();

// DataSet (legacy)
var dataSet = new SqlDataAdapter(command).Fill(new DataSet());

// Async versions
var reader = await command.ExecuteReaderAsync();
int affected = await command.ExecuteNonQueryAsync();
var value = await command.ExecuteScalarAsync();
```

---

## Comparison: Raw SQL vs EF vs Dapper

| Aspect | Raw SQL | Dapper | EF Core |
|--------|---------|--------|---------|
| **Performance** | 100% (baseline) | 95% | 70% |
| **Code Size** | Huge | Small | Tiny |
| **Type Safety** | ❌ None | ⚠️ Partial | ✅ Strong |
| **Learning Curve** | Easy | Easy | Hard |
| **Maintenance** | Hard | Medium | Easy |
| **Flexibility** | 100% | 99% | 80% |
| **SQL Control** | 100% | 100% | 50% |
| **Relationships** | Manual | Manual | Auto |
| **Use Case** | Extreme perf | Performance | CRUD |

---

## In the EHR Codebase

Raw SQL might be used for:

```csharp
// Performance-critical report
SELECT TOP 100 p.Id, p.Name, COUNT(a.Id) as AppointmentCount
FROM Patients p
LEFT JOIN Appointments a ON p.Id = a.PatientId
GROUP BY p.Id, p.Name
ORDER BY AppointmentCount DESC;

// Compliance audit (complex logic)
SELECT * FROM AuditLog
WHERE CreatedAt >= @StartDate
  AND EntityType = @EntityType
  AND (UserId = @UserId OR @UserId IS NULL);

// Maintenance (one-time script)
UPDATE Patients SET Status = 'Archived'
WHERE LastVisitDate < DATEADD(YEAR, -5, GETDATE());
```

---

## Three-Tier Approach

```
Application Queries
        ↓
    Decision
    ┌─────┴──────┬────────────┐
    ↓            ↓            ↓
Standard CRUD  Performance  One-Time
   ↓           Critical      Scripts
   ↓              ↓            ↓
  EF Core      Dapper      Raw SQL

 80% of        15% of      5% of
 queries       queries     queries
```

**Strategy:**
1. Start with **EF Core** for CRUD
2. Use **Dapper** when EF slow
3. Use **Raw SQL** only when extreme performance needed or one-time scripts

---

## Best Practices

✅ **DO:**
- Use parameterized queries
- Use connection pooling
- Close connections properly (`using` statement)
- Handle null values
- Use async methods
- Comment complex SQL

❌ **DON'T:**
- String concatenation
- Hardcode credentials
- Leave connections open
- Assume column order
- Mix sync and async
- Write SQL directly in classes (use stored procedures or SQL files)

---

## Common Patterns

### Pattern 1: Safe Parameterized Query
```csharp
var sql = "SELECT * FROM Patients WHERE Email = @Email";
command.Parameters.AddWithValue("@Email", userEmail);
// Safe even if userEmail = "'; DROP TABLE--"
```

### Pattern 2: Connection Pooling
```csharp
// Reuse connection string for pooling
const string connectionString = "connection-string;Max Pool Size=100;";
using var connection = new SqlConnection(connectionString);
```

### Pattern 3: Transaction
```csharp
using var transaction = connection.BeginTransaction();
try
{
    command1.Transaction = transaction;
    command2.Transaction = transaction;
    
    await command1.ExecuteNonQueryAsync();
    await command2.ExecuteNonQueryAsync();
    
    transaction.Commit();
}
catch
{
    transaction.Rollback();
}
```

---

## Interview Questions

**Q: When would you use raw SQL over EF or Dapper?**

A: For extreme performance requirements, complex database features, or one-time maintenance scripts where the overhead of ORM isn't worth it.

**Q: How to prevent SQL Injection in raw SQL?**

A: Always use parameterized queries. Never concatenate user input into SQL strings.

**Q: Performance comparison?**

A: Raw SQL 100%, Dapper 95%, EF 70%. For most apps, difference negligible. Use Dapper/EF for productivity.

**Q: What's the risk of raw SQL?**

A: Maintenance burden, SQL injection if not careful, hard to track changes, verbose code.

---

## Related Documentation

- **Entity Framework:** ../EntityFramework/README.md
- **Dapper:** ../Dapper/README.md
- **ORM Comparison:** ../orm-comparison.md
- **Hybrid Approach:** ../Hybrid/README.md

---

## Next Steps

1. **Understand when to use:** why-raw-sql.md
2. **Learn fundamentals:** raw-sql-fundamentals.md
3. **Study patterns:** query-patterns.md
4. **Security first:** security-best-practices.md
5. **Interview prep:** interview-qa.md

---

## Quick Reference

```csharp
// Complete pattern
using var connection = new SqlConnection(connectionString);
using var command = new SqlCommand(sql, connection);

command.Parameters.AddWithValue("@Param", value);

await connection.OpenAsync();

if (sql.Contains("SELECT"))
{
    using var reader = await command.ExecuteReaderAsync();
    while (await reader.ReadAsync())
    {
        // Process row
    }
}
else
{
    int affected = await command.ExecuteNonQueryAsync();
}
```
