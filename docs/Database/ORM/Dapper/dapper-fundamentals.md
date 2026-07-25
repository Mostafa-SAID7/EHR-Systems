# Dapper Fundamentals

## What You Need

### 1. NuGet Package
```bash
dotnet add package Dapper
```

### 2. Connection String
```csharp
var connectionString = "Server=localhost;Database=EHR;...";
var connection = new SqlConnection(connectionString);
```

### 3. Model Class
```csharp
public class Patient
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string MRN { get; set; }
    public string Email { get; set; }
    public DateTime CreatedAt { get; set; }
}
```

That's it. You're ready to use Dapper.

---

## Core Methods

### 1. QueryAsync<T> - Get Multiple Objects

```csharp
// SELECT returns IEnumerable<T>
var patients = await connection.QueryAsync<Patient>(
    "SELECT Id, Name, MRN, Email FROM Patients"
);

foreach (var patient in patients)
{
    Console.WriteLine(patient.Name);
}
```

### 2. QueryFirstOrDefaultAsync<T> - Get One or Null

```csharp
// SELECT WHERE... returns first match or null
var patient = await connection.QueryFirstOrDefaultAsync<Patient>(
    "SELECT * FROM Patients WHERE Id = @Id",
    new { Id = 1 }
);

if (patient != null)
    Console.WriteLine(patient.Name);
else
    Console.WriteLine("Not found");
```

### 3. QuerySingleAsync<T> - Get Exactly One

```csharp
// SELECT WHERE... returns exactly one, throws if 0 or 2+
var patient = await connection.QuerySingleAsync<Patient>(
    "SELECT * FROM Patients WHERE Id = @Id",
    new { Id = 1 }
);
// Throws if not found or multiple found
```

### 4. ExecuteAsync - Run INSERT/UPDATE/DELETE

```csharp
// Returns rows affected
var rowsAffected = await connection.ExecuteAsync(
    "INSERT INTO Patients (Name, MRN, Email) VALUES (@Name, @MRN, @Email)",
    new { Name = "Ahmed", MRN = "123456", Email = "ahmed@test.com" }
);

Console.WriteLine($"Inserted {rowsAffected} rows");
```

### 5. ExecuteScalarAsync<T> - Get Single Value

```csharp
// SELECT COUNT(*), MAX(Id), etc - returns scalar value
var count = await connection.ExecuteScalarAsync<int>(
    "SELECT COUNT(*) FROM Patients"
);

Console.WriteLine($"Total patients: {count}");
```

---

## Parameter Binding

### Named Parameters

```csharp
// ✅ Safe - Parameters prevent SQL injection
var patient = await connection.QueryFirstOrDefaultAsync<Patient>(
    "SELECT * FROM Patients WHERE Email = @Email AND Status = @Status",
    new { Email = "user@example.com", Status = "Active" }
);

// Parameter names must match @Name in SQL
```

### Multiple Parameter Objects

```csharp
// Anonymous object - property names become parameter names
var patient = await connection.QueryFirstOrDefaultAsync<Patient>(
    "SELECT * FROM Patients WHERE Id = @Id AND Active = @Active",
    new 
    { 
        Id = 1, 
        Active = true 
    }
);
```

### Complex Types as Parameters

```csharp
var patient = new Patient 
{ 
    Id = 1, 
    Name = "Ahmed", 
    Email = "ahmed@test.com" 
};

var updated = await connection.ExecuteAsync(
    @"UPDATE Patients 
      SET Name = @Name, Email = @Email 
      WHERE Id = @Id",
    patient  // Entire object as parameters
);
```

### Null Parameters

```csharp
// Null is passed as DBNull.Value
var results = await connection.QueryAsync<Patient>(
    "SELECT * FROM Patients WHERE Email = @Email OR PhoneNumber IS NULL",
    new { Email = email }  // If null, passed as DBNull
);
```

---

## Column-to-Property Mapping

### Exact Match (Automatic)

```csharp
// SQL columns match C# properties exactly (case-insensitive)
var patients = await connection.QueryAsync<Patient>(
    "SELECT Id, Name, MRN, Email, CreatedAt FROM Patients"
);
// Automatic mapping works!
```

### Mismatch (Must Handle)

```csharp
// SQL returns different column names
var patients = await connection.QueryAsync<PatientDto>(
    "SELECT Id, FirstName as Name, MRN FROM Patients"
);
// SQL "FirstName" → PatientDto.Name ✅

// Or use SELECT with aliases
var patients = await connection.QueryAsync<Patient>(
    "SELECT PatientId as Id, PatientName as Name FROM Patients"
);
```

### Extra Columns (Ignored)

```csharp
// SQL returns more columns than C# properties - ignored
var patients = await connection.QueryAsync<Patient>(
    "SELECT Id, Name, MRN, Email, CreatedAt, UpdatedAt, IsDeleted FROM Patients"
);
// Extra columns ignored, no error
```

### Missing Columns (Null)

```csharp
// SQL returns fewer columns than C# properties - null
var patients = await connection.QueryAsync<Patient>(
    "SELECT Id, Name FROM Patients"
);
// MRN = null, Email = null
```

---

## Execution Options

### Command Timeout

```csharp
// Default 30 seconds, can override
var patients = await connection.QueryAsync<Patient>(
    sql,
    commandTimeout: 60  // 60 seconds
);
```

### Transaction Support

```csharp
using var transaction = connection.BeginTransaction();
try
{
    await connection.ExecuteAsync(
        "INSERT INTO Patients...",
        parameters,
        transaction: transaction  // Pass transaction
    );
    
    await connection.ExecuteAsync(
        "UPDATE Appointments...",
        parameters,
        transaction: transaction
    );
    
    transaction.Commit();
}
catch
{
    transaction.Rollback();
    throw;
}
```

### Command Type (Stored Procedures)

```csharp
// SQL (default)
var patients = await connection.QueryAsync<Patient>(
    "SELECT * FROM Patients WHERE Status = @Status"
);

// Stored Procedure
var patients = await connection.QueryAsync<Patient>(
    "sp_GetPatients",
    new { Status = "Active" },
    commandType: CommandType.StoredProcedure
);

// Text (default, explicit)
var patients = await connection.QueryAsync<Patient>(
    "SELECT * FROM Patients",
    commandType: CommandType.Text
);
```

---

## Common Patterns

### Pattern 1: Safe Parameter Query

```csharp
public async Task<Patient> GetPatientByEmailAsync(string email)
{
    // ✅ Always use parameters, never concatenate
    return await _connection.QueryFirstOrDefaultAsync<Patient>(
        "SELECT * FROM Patients WHERE Email = @Email",
        new { Email = email }  // Safe even if email = "'; DROP TABLE--"
    );
}
```

### Pattern 2: Filter with Conditions

```csharp
public async Task<IEnumerable<Patient>> SearchPatientsAsync(
    string name = null, 
    string status = null,
    DateTime? startDate = null)
{
    var sql = "SELECT * FROM Patients WHERE 1=1";
    var parameters = new DynamicParameters();
    
    if (!string.IsNullOrEmpty(name))
    {
        sql += " AND Name LIKE @Name";
        parameters.Add("@Name", $"%{name}%");
    }
    
    if (!string.IsNullOrEmpty(status))
    {
        sql += " AND Status = @Status";
        parameters.Add("@Status", status);
    }
    
    if (startDate.HasValue)
    {
        sql += " AND CreatedAt >= @StartDate";
        parameters.Add("@StartDate", startDate);
    }
    
    return await _connection.QueryAsync<Patient>(sql, parameters);
}
```

### Pattern 3: INSERT with Generated ID

```csharp
public async Task<int> CreatePatientAsync(Patient patient)
{
    // SQL Server: Use SCOPE_IDENTITY() to get inserted ID
    var sql = @"
        INSERT INTO Patients (Name, MRN, Email) 
        VALUES (@Name, @MRN, @Email);
        SELECT CAST(SCOPE_IDENTITY() as int)";
    
    var newId = await _connection.QuerySingleAsync<int>(sql, patient);
    return newId;
}
```

### Pattern 4: UPDATE with Change Detection

```csharp
public async Task<bool> UpdatePatientAsync(Patient patient)
{
    var sql = @"
        UPDATE Patients 
        SET Name = @Name, Email = @Email, Status = @Status, UpdatedAt = @UpdatedAt
        WHERE Id = @Id";
    
    var rowsAffected = await _connection.ExecuteAsync(sql, new
    {
        patient.Name,
        patient.Email,
        patient.Status,
        UpdatedAt = DateTime.UtcNow,
        patient.Id
    });
    
    return rowsAffected > 0;  // True if updated, false if not found
}
```

### Pattern 5: DELETE

```csharp
public async Task<bool> DeletePatientAsync(int id)
{
    var rowsAffected = await _connection.ExecuteAsync(
        "DELETE FROM Patients WHERE Id = @Id",
        new { Id = id }
    );
    
    return rowsAffected > 0;  // True if deleted, false if not found
}
```

---

## Dynamic Parameters

For complex queries with optional parameters:

```csharp
public async Task<IEnumerable<Patient>> SearchAsync(
    string name = null,
    string status = null,
    int? minAge = null)
{
    var parameters = new DynamicParameters();
    var sql = "SELECT * FROM Patients WHERE 1=1";
    
    if (!string.IsNullOrEmpty(name))
    {
        sql += " AND Name LIKE @Name";
        parameters.Add("@Name", $"%{name}%");
    }
    
    if (!string.IsNullOrEmpty(status))
    {
        sql += " AND Status = @Status";
        parameters.Add("@Status", status);
    }
    
    if (minAge.HasValue)
    {
        sql += " AND DATEDIFF(YEAR, DOB, GETDATE()) >= @MinAge";
        parameters.Add("@MinAge", minAge);
    }
    
    return await _connection.QueryAsync<Patient>(sql, parameters);
}
```

---

## Error Handling

### Connection Errors

```csharp
try
{
    var patients = await connection.QueryAsync<Patient>(
        "SELECT * FROM Patients"
    );
}
catch (SqlException ex) when (ex.Number == -2)
{
    Console.WriteLine("Timeout");
}
catch (SqlException ex)
{
    Console.WriteLine($"Database error: {ex.Message}");
}
catch (Exception ex)
{
    Console.WriteLine($"Unexpected error: {ex.Message}");
}
```

### Query Syntax Errors

```csharp
// Dapper doesn't validate SQL - errors at runtime
var patients = await connection.QueryAsync<Patient>(
    "SELECT * FORM Patients"  // Typo: FORM instead of FROM
);
// Throws SqlException: "Incorrect syntax near keyword 'FROM'"
```

---

## Best Practices

✅ **DO:**
- Use `await` with async methods (Async suffix)
- Use parameterized queries (prevent SQL injection)
- Close connections (use `using`)
- Handle exceptions (SqlException common)
- Use specific methods (QueryFirstOrDefault vs Query)

❌ **DON'T:**
- String concatenation in SQL
- Ignore null values
- Assume column order
- Mix async and sync
- Forget transactions for multiple operations

---

## Interview Q&A

**Q: What's Dapper?**

A: A lightweight ORM that maps SQL query results to C# objects. Minimal overhead, direct SQL control.

**Q: How to prevent SQL Injection?**

A: Use parameterized queries with @ParameterName syntax. Never concatenate user input.

**Q: Difference between Query and QueryFirstOrDefault?**

A:
- Query: Returns IEnumerable, multiple rows
- QueryFirstOrDefault: Returns single object or null

**Q: Can you use transactions?**

A: Yes, pass transaction object:
```csharp
await connection.ExecuteAsync(sql, param, transaction: transaction);
```

**Q: Performance vs Entity Framework?**

A: Dapper 10-20% faster on simple queries, 5-10x faster on complex reports because no query translation.

---

## Related Files

- **why-dapper.md** - When to use Dapper
- **query-patterns.md** - SELECT, INSERT, UPDATE, DELETE examples
- **advanced-features.md** - Multi-mapping, stored procedures
- **interview-qa.md** - Complete interview Q&A
