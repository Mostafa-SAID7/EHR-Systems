# Why Use Entity Framework?

## ORM Concept

An ORM (Object-Relational Mapper) bridges the gap between **objects in your code** and **rows in the database**.

```
Your C# Code        Entity Framework        Database
┌──────────┐               ┌──┐               ┌────┐
│  User    │◄─────────────►│EF│◄─────────────►│User│
│ object   │  Translation  └──┘   SQL Queries  table
└──────────┘                      Generated    └────┘
```

---

## Alternative: Raw SQL

### Without EF (Raw SQL)
```csharp
using var connection = new SqlConnection("connection-string");
await connection.OpenAsync();

var command = connection.CreateCommand();
command.CommandText = "SELECT * FROM Users WHERE Id = @Id";
command.Parameters.AddWithValue("@Id", 1);

using var reader = await command.ExecuteReaderAsync();
if (await reader.ReadAsync())
{
    var user = new User
    {
        Id = reader.GetInt32(0),
        Email = reader.GetString(1),
        Name = reader.GetString(2)
    };
}
```

### With EF (Object-Oriented)
```csharp
var user = await context.Users
    .FirstAsync(u => u.Id == 1);
```

**EF is 10x simpler!**

---

## Key Advantages

### 1. LINQ - Type-Safe Queries

```csharp
// ✅ Type-safe, compile-time checking
var users = context.Users
    .Where(u => u.Status == "Active")
    .OrderBy(u => u.CreatedAt)
    .ToList();

// ❌ Raw SQL - string-based, error-prone
var users = connection.ExecuteQuery(
    "SELECT * FROM Users WHERE Status = 'Active' ORDER BY CreatedAt"
);
// Typo in column name? Found at runtime only!
```

### 2. Change Tracking - Automatic Detection

```csharp
var user = await context.Users.FirstAsync(u => u.Id == 1);
user.Email = "newemail@example.com";
user.Status = "Inactive";

await context.SaveChangesAsync();
// EF detects exactly what changed and generates:
// UPDATE Users SET Email = 'newemail@example.com', Status = 'Inactive' WHERE Id = 1
```

Without EF:
```csharp
// Manual: Tell database what changed
await connection.ExecuteAsync(
    "UPDATE Users SET Email = @Email, Status = @Status WHERE Id = @Id",
    new { Email = "newemail@example.com", Status = "Inactive", Id = 1 }
);
```

### 3. Migrations - Schema Versioning

```bash
# Create a migration
dotnet ef migrations add AddPhoneNumberColumn

# EF generates a migration file that:
# - Documents the change
# - Can be applied to any database
# - Can be rolled back if needed
```

Without EF:
```sql
-- Manual SQL scripts, hard to version/rollback
ALTER TABLE Users ADD PhoneNumber NVARCHAR(20);
-- How do you rollback? No automatic version control
```

### 4. Relationships - Foreign Key Handling

```csharp
// ✅ EF handles JOIN automatically
var user = await context.Users
    .Include(u => u.Orders)      // Load related Orders
    .Include(u => u.Addresses)   // Load related Addresses
    .FirstAsync(u => u.Id == 1);

var firstOrder = user.Orders.First(); // No additional query needed
```

Without EF:
```sql
-- Manual JOIN writing
SELECT u.*, o.*, a.*
FROM Users u
LEFT JOIN Orders o ON u.Id = o.UserId
LEFT JOIN Addresses a ON u.Id = a.UserId
WHERE u.Id = 1

-- Then manually parse results and populate objects
```

### 5. Async/Await Built-In

```csharp
// ✅ EF async - doesn't block thread
var users = await context.Users
    .Where(u => u.Active)
    .ToListAsync(); // Awaitable

// ❌ Raw SQL without async
var users = context.Users
    .Where(u => u.Active)
    .ToList(); // Blocks thread
```

---

## When NOT to Use EF

### 1. Complex Reports with Many JOINs

```csharp
// ❌ EF gets complicated with 5+ table JOINs
var result = await context.Users
    .Include(u => u.Orders)
        .ThenInclude(o => o.Items)
        .ThenInclude(i => i.Product)
    .Include(u => u.Addresses)
    .Include(u => u.Payments)
    .Where(u => u.Status == "Active")
    .Select(u => new ReportDto { /* ... */ })
    .ToListAsync();

// ✅ Raw SQL is clearer
var result = await context.Database.SqlQuery<ReportDto>(
    @"SELECT u.Id, u.Name, COUNT(o.Id) as OrderCount, SUM(oi.Amount) as Total
      FROM Users u
      LEFT JOIN Orders o ON u.Id = o.UserId
      LEFT JOIN OrderItems oi ON o.Id = oi.OrderId
      WHERE u.Status = 'Active'
      GROUP BY u.Id, u.Name"
).ToListAsync();
```

### 2. Bulk Operations (Insert/Update 1M+ rows)

```csharp
// ❌ EF slow for 1 million inserts
foreach (var record in millionRecords)
{
    context.Users.Add(record);
}
await context.SaveChangesAsync(); // Very slow!

// ✅ Batch insert with SQL
await context.Database.ExecuteSqlAsync(
    "INSERT INTO Users (Name, Email) VALUES (@Name, @Email)",
    parameters
); // Much faster
```

### 3. Simple Read-Only Queries

```csharp
// ❌ EF overhead for simple select
var users = await context.Users
    .AsNoTracking()
    .ToListAsync();

// ✅ Raw SQL simpler
var users = await context.Database.SqlQuery<User>(
    "SELECT * FROM Users"
).ToListAsync();
```

---

## EF Performance: Good vs Bad

### ✅ Good Performance

```csharp
// Eager load what you need
var users = await context.Users
    .AsNoTracking() // No tracking overhead
    .Include(u => u.Orders) // Load once
    .Select(u => new UserDto // Only needed fields
    {
        Id = u.Id,
        Email = u.Email,
        OrderCount = u.Orders.Count
    })
    .ToListAsync();
// 1 query, fast
```

### ❌ Bad Performance

```csharp
// N+1 query problem
var users = await context.Users.ToListAsync();
foreach (var user in users)
{
    user.Orders = await context.Orders // Query per user!
        .Where(o => o.UserId == user.Id)
        .ToListAsync();
}
// N+1 queries = slow
```

---

## Interview Answer Template

**Q: Why did you choose Entity Framework for your project?**

A: "I chose EF because:

1. **Type-safety** - LINQ provides compile-time checking vs error-prone SQL strings
2. **Productivity** - Less boilerplate code, automatic change tracking, migrations
3. **Maintainability** - Domain models are in C#, easier to refactor
4. **Async built-in** - Clean async/await without manual threading
5. **Database agnostic** - Works with SQL Server, MySQL, PostgreSQL

However, for complex reports with 5+ JOINs or bulk operations with 1M+ rows, we use raw SQL for better performance."

---

## When to Stick with Raw SQL

- Reporting/Analytics queries
- Bulk operations
- Complex business logic queries
- Performance-critical reads
- Database-specific optimizations

**Best Practice: Use EF for 95% of queries, raw SQL for 5% that need optimization.**
