# Raw SQL Security & Best Practices

## SQL Injection Prevention - Critical!

### ❌ VULNERABLE - String Concatenation

```csharp
// NEVER DO THIS
string email = GetUserInput(); // user enters: '; DROP TABLE Patients; --
string sql = $"SELECT * FROM Patients WHERE Email = '{email}'";
// Becomes: SELECT * FROM Patients WHERE Email = ''; DROP TABLE Patients; --'
// TABLE DELETED!
```

### ✅ SAFE - Parameterized Queries

```csharp
string email = GetUserInput();
string sql = "SELECT * FROM Patients WHERE Email = @Email";
command.Parameters.AddWithValue("@Email", email);
// Safe: Parameter treated as data, not SQL
```

---

## Parameter Types

### Correct - Named Parameters
```csharp
var sql = "SELECT * FROM Patients WHERE Email = @Email AND Status = @Status";
command.Parameters.AddWithValue("@Email", email);
command.Parameters.AddWithValue("@Status", "Active");
```

### Correct - Positional Parameters
```csharp
var sql = "SELECT * FROM Patients WHERE Email = ? AND Status = ?";
// Different databases use different syntax
// @Name (SQL Server)
// ? (MySQL, PostgreSQL)
// : (Oracle)
```

### Wrong - String Interpolation
```csharp
// ❌ DANGEROUS
var sql = $"SELECT * FROM Patients WHERE Email = '{email}'";

// ✅ SAFE
var sql = "SELECT * FROM Patients WHERE Email = @Email";
```

---

## Common Injection Attacks & Defenses

### Attack 1: Authentication Bypass

```csharp
// User enters: admin' OR '1'='1
// ❌ Vulnerable
var sql = $"SELECT * FROM Users WHERE Username = '{username}' AND Password = '{password}'";
// Becomes: SELECT * FROM Users WHERE Username = 'admin' OR '1'='1' AND Password = '...'
// Returns first row (any user!)

// ✅ Safe with parameters
var sql = "SELECT * FROM Users WHERE Username = @Username AND Password = @Password";
command.Parameters.AddWithValue("@Username", username);
command.Parameters.AddWithValue("@Password", password);
```

### Attack 2: Data Extraction

```csharp
// User enters: ' UNION SELECT password FROM Users WHERE '1'='1
// ❌ Vulnerable
var sql = $"SELECT Id, Name FROM Patients WHERE Name LIKE '{searchTerm}%'";
// Attacker extracts password column

// ✅ Safe
var sql = "SELECT Id, Name FROM Patients WHERE Name LIKE @SearchTerm";
command.Parameters.AddWithValue("@SearchTerm", $"{searchTerm}%");
```

### Attack 3: Stacked Queries

```csharp
// User enters: 1); DELETE FROM Patients; --
// ❌ Vulnerable
var sql = $"UPDATE Patients SET Status = 'Active' WHERE Id = {id}";
// Becomes: UPDATE Patients SET Status = 'Active' WHERE Id = 1); DELETE FROM Patients; --'

// ✅ Safe
var sql = "UPDATE Patients SET Status = @Status WHERE Id = @Id";
command.Parameters.AddWithValue("@Status", "Active");
command.Parameters.AddWithValue("@Id", id);
```

---

## Parameter Type Safety

### Explicit Type Definition

```csharp
// ✅ Better - Explicitly typed
var param = new SqlParameter("@Age", SqlDbType.Int)
{
    Value = age
};
command.Parameters.Add(param);

// vs

// ⚠️ Inferred type
command.Parameters.AddWithValue("@Age", age);
// Type inferred from object (could be string, int, etc)
```

### Handling Null

```csharp
// ✅ Explicit null
if (phoneNumber == null)
    command.Parameters.AddWithValue("@Phone", DBNull.Value);
else
    command.Parameters.AddWithValue("@Phone", phoneNumber);

// vs

// ❌ Dangerous null concatenation
var sql = $"... WHERE Phone = '{phoneNumber}'"; // null.ToString() = "null" string!
```

---

## Input Validation

### Validate Before Database

```csharp
public class PatientValidator
{
    public void Validate(CreatePatientRequest request)
    {
        // Validate length
        if (request.Name?.Length > 200)
            throw new ValidationException("Name too long");
        
        // Validate format
        if (!Regex.IsMatch(request.Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            throw new ValidationException("Invalid email");
        
        // Validate range
        if (request.Age < 0 || request.Age > 150)
            throw new ValidationException("Invalid age");
    }
}

// Then use validated data
command.Parameters.AddWithValue("@Name", validatedRequest.Name);
```

---

## Connection Security

### Store Credentials Safely

```csharp
// ❌ BAD - Hardcoded
const string connectionString = "Server=localhost;Password=admin123;";

// ✅ GOOD - Configuration
var connectionString = configuration["ConnectionStrings:EHR"];

// ✅ BETTER - User Secrets (development)
// dotnet user-secrets set "ConnectionStrings:EHR" "..."

// ✅ BEST - Azure Key Vault (production)
var credential = new DefaultAzureCredential();
var client = new SecretClient(new Uri("https://vault.azure.net/"), credential);
var secret = client.GetSecret("EhrConnectionString");
var connectionString = secret.Value.Value;
```

### Connection String Security

```csharp
// Include security features
string connectionString = @"
    Server=production-server.database.windows.net;
    Database=EHR;
    User Id=produser;
    Password=ComplexPassword123!;
    Encrypt=yes;
    TrustServerCertificate=no;
    Connection Timeout=30;
    Max Pool Size=100;
    Application Name=EHRApp;
    ";
```

---

## Output Encoding

### Prevent XSS (if displaying results in web)

```csharp
// ❌ BAD - Display raw from database
var name = reader.GetString("Name"); // "Ahmed<script>alert('xss')</script>"
return $"<p>Hello {name}</p>"; // Executes script!

// ✅ GOOD - HTML encode
using System.Web;
var name = reader.GetString("Name");
return $"<p>Hello {HttpUtility.HtmlEncode(name)}</p>"; // Safe
```

---

## Error Handling

### Don't Expose Internal Details

```csharp
// ❌ BAD - Reveals database info
try
{
    await command.ExecuteNonQueryAsync();
}
catch (SqlException ex)
{
    return BadRequest(ex.Message); // Shows database error details
}

// ✅ GOOD - Generic message
try
{
    await command.ExecuteNonQueryAsync();
}
catch (SqlException ex)
{
    _logger.LogError("Database error: {Message}", ex.Message);
    return BadRequest("Operation failed");
}
```

---

## Stored Procedures

### Safer Than Dynamic SQL

```csharp
// ✅ BETTER - Stored procedure with parameters
var command = new SqlCommand("sp_GetPatient", connection)
{
    CommandType = CommandType.StoredProcedure
};
command.Parameters.AddWithValue("@PatientId", id);
// SQL Server executes stored procedure, not dynamic SQL

// vs

// ❌ WORSE - Dynamic SQL
var command = new SqlCommand($"SELECT * FROM Patients WHERE Id = {id}", connection);
// Vulnerable if id from user input
```

### Stored Procedure Parameter Types

```csharp
// Stored procedure ensures parameters are typed
var command = new SqlCommand("sp_CreatePatient", connection)
{
    CommandType = CommandType.StoredProcedure
};

// Parameters strongly typed in procedure definition
command.Parameters.Add(new SqlParameter
{
    ParameterName = "@Name",
    SqlDbType = SqlDbType.NVarChar,
    Size = 200,
    Value = name
});
```

---

## Audit & Logging

### Log Database Operations

```csharp
public class AuditedCommand
{
    private readonly ILogger _logger;
    private readonly SqlCommand _command;
    
    public async Task<int> ExecuteAsync()
    {
        _logger.LogInformation(
            "Executing SQL: {CommandText} with parameters: {Parameters}",
            _command.CommandText,
            string.Join(", ", _command.Parameters.Cast<SqlParameter>()
                .Select(p => $"{p.ParameterName}={p.Value}"))
        );
        
        try
        {
            var result = await _command.ExecuteNonQueryAsync();
            _logger.LogInformation("Execution successful: {RowsAffected} rows", result);
            return result;
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "Database error executing: {CommandText}", 
                _command.CommandText);
            throw;
        }
    }
}
```

---

## Security Checklist

✅ **Always:**
- [ ] Use parameterized queries
- [ ] Validate input (length, format, range)
- [ ] Store credentials in configuration
- [ ] Log database operations
- [ ] Use TLS for connections
- [ ] Set appropriate permissions (least privilege)
- [ ] Review SQL for security issues
- [ ] Use stored procedures for complex logic
- [ ] Dispose resources properly
- [ ] Test with malicious input

❌ **Never:**
- [ ] Concatenate user input into SQL
- [ ] Hardcode credentials
- [ ] Trust user input
- [ ] Display raw database errors
- [ ] Use SELECT *
- [ ] Leave transactions open
- [ ] Use weak passwords
- [ ] Assume string input is safe
- [ ] Mix data and executable code
- [ ] Expose database structure

---

## Common Mistakes

### Mistake 1: Numeric Injection

```csharp
// ❌ Vulnerable
var sql = $"SELECT * FROM Patients WHERE Id = {id}"; // User: 1 OR 1=1

// ✅ Safe (even for numbers)
var sql = "SELECT * FROM Patients WHERE Id = @Id";
command.Parameters.AddWithValue("@Id", id);
```

### Mistake 2: LIKE Injection

```csharp
// ❌ Vulnerable
var sql = $"SELECT * FROM Patients WHERE Name LIKE '%{searchTerm}%'";

// ✅ Safe
var sql = "SELECT * FROM Patients WHERE Name LIKE @SearchTerm";
command.Parameters.AddWithValue("@SearchTerm", $"%{searchTerm}%");
```

### Mistake 3: Escaped Quotes

```csharp
// ❌ False sense of security
var escaped = input.Replace("'", "''");
var sql = $"SELECT * FROM Patients WHERE Name = '{escaped}'"; // Still vulnerable!

// ✅ Actually safe
var sql = "SELECT * FROM Patients WHERE Name = @Name";
command.Parameters.AddWithValue("@Name", input);
```

---

## Interview Q&A

**Q: How do you prevent SQL Injection?**

A: Always use parameterized queries. Parameters are treated as data, never as executable SQL.

**Q: What's the difference between parameterized and escaped quotes?**

A: Parameterization is safe. Escaping quotes only prevents quote-based injection, not all injection types.

**Q: Should you validate input before database?**

A: Yes - both for security and business logic. Validate early, fail fast.

**Q: How to handle null values safely?**

A: Use DBNull.Value instead of null, and check IsDBNull before reading values.

**Q: Best practice for error handling?**

A: Log full errors internally, return generic messages to users to avoid exposing database structure.

---

## Resources

- OWASP SQL Injection: https://owasp.org/www-community/attacks/SQL_Injection
- Microsoft Parameterized Queries: https://docs.microsoft.com/en-us/archive/msdn-magazine/2008/april/prevent-sql-injection-attacks
- SQL Injection Prevention Cheat Sheet: https://cheatsheetseries.owasp.org/cheatsheets/SQL_Injection_Prevention_Cheat_Sheet.html
