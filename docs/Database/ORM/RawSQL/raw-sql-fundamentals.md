# Raw SQL Fundamentals - ADO.NET

## Core Components

### SqlConnection
```csharp
// Create connection
var connection = new SqlConnection("Server=localhost;Database=EHR;...");

// Using statement (auto-dispose)
using var connection = new SqlConnection(connectionString);

// Connection pooling (automatic with connection strings)
// Same connection string = reused from pool
var conn1 = new SqlConnection(connectionString); // From pool
var conn2 = new SqlConnection(connectionString); // From pool
```

### SqlCommand
```csharp
// Create command
var command = new SqlCommand(sql, connection);

// Or set command text later
command.CommandText = sql;
command.CommandType = CommandType.Text; // Default

// Timeout
command.CommandTimeout = 30; // seconds
```

### SqlParameter
```csharp
// Add parameter (safest)
command.Parameters.AddWithValue("@Id", value);

// Or create explicitly
var param = new SqlParameter("@Id", SqlDbType.Int);
param.Value = value;
command.Parameters.Add(param);

// Typed parameter (better)
command.Parameters.Add(new SqlParameter
{
    ParameterName = "@Id",
    SqlDbType = SqlDbType.Int,
    Value = value
});
```

---

## Query Patterns

### SELECT Single Row
```csharp
var sql = "SELECT * FROM Patients WHERE Id = @Id";
using var connection = new SqlConnection(connectionString);
using var command = new SqlCommand(sql, connection);
command.Parameters.AddWithValue("@Id", 1);

await connection.OpenAsync();
using var reader = await command.ExecuteReaderAsync();

Patient patient = null;
if (await reader.ReadAsync())
{
    patient = new Patient
    {
        Id = reader.GetInt32(0),
        Name = reader.GetString(1),
        Email = reader.GetString(2)
    };
}
```

### SELECT Multiple Rows
```csharp
var sql = "SELECT Id, Name, Email FROM Patients WHERE Status = @Status";
using var connection = new SqlConnection(connectionString);
using var command = new SqlCommand(sql, connection);
command.Parameters.AddWithValue("@Status", "Active");

await connection.OpenAsync();
var patients = new List<Patient>();

using var reader = await command.ExecuteReaderAsync();
while (await reader.ReadAsync())
{
    patients.Add(new Patient
    {
        Id = reader.GetInt32(0),
        Name = reader.GetString(1),
        Email = reader.GetString(2)
    });
}
```

### SELECT with COUNT
```csharp
var sql = "SELECT COUNT(*) FROM Patients WHERE Status = @Status";
using var connection = new SqlConnection(connectionString);
using var command = new SqlCommand(sql, connection);
command.Parameters.AddWithValue("@Status", "Active");

await connection.OpenAsync();
var count = (int)await command.ExecuteScalarAsync();
Console.WriteLine($"Total active patients: {count}");
```

### INSERT
```csharp
var sql = @"
    INSERT INTO Patients (Name, Email, MRN)
    VALUES (@Name, @Email, @MRN);
    SELECT CAST(SCOPE_IDENTITY() as int)";

using var connection = new SqlConnection(connectionString);
using var command = new SqlCommand(sql, connection);
command.Parameters.AddWithValue("@Name", "Ahmed");
command.Parameters.AddWithValue("@Email", "ahmed@test.com");
command.Parameters.AddWithValue("@MRN", "123456");

await connection.OpenAsync();
var newId = (int)await command.ExecuteScalarAsync();
```

### UPDATE
```csharp
var sql = @"
    UPDATE Patients
    SET Name = @Name, Email = @Email, UpdatedAt = @Now
    WHERE Id = @Id";

using var connection = new SqlConnection(connectionString);
using var command = new SqlCommand(sql, connection);
command.Parameters.AddWithValue("@Name", "Ahmed Updated");
command.Parameters.AddWithValue("@Email", "updated@test.com");
command.Parameters.AddWithValue("@Now", DateTime.UtcNow);
command.Parameters.AddWithValue("@Id", 1);

await connection.OpenAsync();
var rowsAffected = await command.ExecuteNonQueryAsync();
Console.WriteLine($"Updated {rowsAffected} rows");
```

### DELETE
```csharp
var sql = "DELETE FROM Patients WHERE Id = @Id";
using var connection = new SqlConnection(connectionString);
using var command = new SqlCommand(sql, connection);
command.Parameters.AddWithValue("@Id", 1);

await connection.OpenAsync();
var rowsAffected = await command.ExecuteNonQueryAsync();
```

---

## SqlDataReader Methods

```csharp
// Get by column index
int id = reader.GetInt32(0);
string name = reader.GetString(1);

// Get by column name
int id = reader.GetInt32("Id");
string name = reader.GetString("Name");

// Get with null checking
int? count = reader.IsDBNull(0) ? (int?)null : reader.GetInt32(0);

// Common type conversions
int intValue = reader.GetInt32(columnIndex);
string strValue = reader.GetString(columnIndex);
decimal decValue = reader.GetDecimal(columnIndex);
bool boolValue = reader.GetBoolean(columnIndex);
DateTime dateValue = reader.GetDateTime(columnIndex);
```

---

## Transactions

### Basic Transaction
```csharp
using var connection = new SqlConnection(connectionString);
await connection.OpenAsync();

using var transaction = connection.BeginTransaction();
try
{
    var command1 = new SqlCommand(
        "UPDATE Patients SET Balance = Balance - 100 WHERE Id = 1",
        connection,
        transaction
    );
    await command1.ExecuteNonQueryAsync();
    
    var command2 = new SqlCommand(
        "UPDATE Accounts SET Balance = Balance + 100 WHERE Id = 1",
        connection,
        transaction
    );
    await command2.ExecuteNonQueryAsync();
    
    await transaction.CommitAsync();
}
catch
{
    await transaction.RollbackAsync();
    throw;
}
```

### Isolation Levels
```csharp
// Read Uncommitted (dirty reads allowed)
var transaction = connection.BeginTransaction(
    IsolationLevel.ReadUncommitted
);

// Read Committed (default)
var transaction = connection.BeginTransaction(
    IsolationLevel.ReadCommitted
);

// Serializable (complete isolation)
var transaction = connection.BeginTransaction(
    IsolationLevel.Serializable
);
```

---

## Connection Pooling

```csharp
// Connection string with pooling
string connectionString = @"
    Server=localhost;
    Database=EHR;
    Integrated Security=true;
    Max Pool Size=100;
    Min Pool Size=5;
    Pooling=true;";

// Pooling automatic - same connection string reuses connections
var conn1 = new SqlConnection(connectionString);
conn1.Open();
conn1.Close(); // Returns to pool

var conn2 = new SqlConnection(connectionString);
conn2.Open(); // May get conn1 from pool
```

---

## Stored Procedures

### Execute Stored Procedure
```csharp
var connection = new SqlConnection(connectionString);
var command = new SqlCommand("sp_GetPatientReport", connection)
{
    CommandType = CommandType.StoredProcedure
};

command.Parameters.AddWithValue("@PatientId", 1);
command.Parameters.AddWithValue("@StartDate", DateTime.Now.AddMonths(-1));

await connection.OpenAsync();
using var reader = await command.ExecuteReaderAsync();

while (await reader.ReadAsync())
{
    // Process results
}
```

### With Output Parameters
```csharp
var command = new SqlCommand("sp_CreatePatient", connection)
{
    CommandType = CommandType.StoredProcedure
};

command.Parameters.AddWithValue("@Name", "Ahmed");
command.Parameters.AddWithValue("@Email", "ahmed@test.com");

var returnValue = new SqlParameter
{
    ParameterName = "@PatientId",
    SqlDbType = SqlDbType.Int,
    Direction = ParameterDirection.Output
};
command.Parameters.Add(returnValue);

await connection.OpenAsync();
await command.ExecuteNonQueryAsync();

int newPatientId = (int)returnValue.Value;
```

---

## DataSet (Legacy)
```csharp
var adapter = new SqlDataAdapter(command);
var dataSet = new DataSet();
adapter.Fill(dataSet);

var table = dataSet.Tables[0];
foreach (DataRow row in table.Rows)
{
    var id = row["Id"];
    var name = row["Name"];
}
```

---

## Error Handling

### SqlException
```csharp
try
{
    await command.ExecuteNonQueryAsync();
}
catch (SqlException ex) when (ex.Number == 2627)
{
    // Unique constraint violation
    Console.WriteLine("Duplicate entry");
}
catch (SqlException ex) when (ex.Number == -2)
{
    // Timeout
    Console.WriteLine("Query timeout");
}
catch (SqlException ex)
{
    Console.WriteLine($"Database error: {ex.Message}");
    Console.WriteLine($"Error code: {ex.Number}");
}
```

### Common Error Codes
```
-2      Timeout
2627    Unique constraint
208     Invalid object name
229     Permission denied
```

---

## Best Practices

✅ **DO:**
- Always use `using` statements for connections/commands
- Always use parameters (prevent SQL injection)
- Use connection pooling (connection strings)
- Handle SqlException specifically
- Use async methods (ExecuteReaderAsync)
- Check IsDBNull before GetValue
- Close/dispose properly

❌ **DON'T:**
- String concatenation in SQL
- Hardcode connection strings (use config)
- Leave connections open
- Assume column order (use column names)
- Mix sync and async
- Catch generic Exception
- Forget to dispose resources

---

## Performance Tips

```csharp
// ✅ Efficient
using var reader = await command.ExecuteReaderAsync(
    CommandBehavior.SequentialAccess | CommandBehavior.SingleRow
);

// Get only needed columns
var sql = "SELECT Id, Name FROM Patients"; // Not SELECT *

// Use connection pooling (automatic)
// Reuse connection strings

// Batch operations
var sql = @"
    INSERT INTO Patients VALUES (...);
    INSERT INTO Patients VALUES (...);"; // All at once
```

---

## Interview Q&A

**Q: What's SqlCommand used for?**

A: Executing SQL queries and commands against a SQL Server database. Supports SELECT, INSERT, UPDATE, DELETE, and stored procedures.

**Q: How to prevent SQL Injection?**

A: Use SqlParameter with AddWithValue or create parameters explicitly. Never concatenate user input into SQL strings.

**Q: SqlDataReader vs DataSet?**

A:
- SqlDataReader: Streaming, forward-only, memory-efficient
- DataSet: Full load, in-memory, allows navigation

**Q: Connection pooling - how does it work?**

A: By default, same connection string reuses connections from pool. Closed connections returned to pool, reopened connections retrieved from pool.

**Q: Async pattern with SqlCommand?**

A: Use ExecuteReaderAsync, ExecuteNonQueryAsync, ExecuteScalarAsync instead of sync versions.

---

## Related Files

- **README.md** - Raw SQL overview
- **why-raw-sql.md** - When to use
- **query-patterns.md** - Advanced patterns
- **security-best-practices.md** - SQL injection prevention
