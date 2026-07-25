# Query Optimization - Performance Tuning

## Performance Problems & Solutions

### Problem 1: Unnecessary Columns (N+1 variant)

```csharp
// ❌ SLOW - Loads all columns
var users = await context.Users.ToListAsync();
// SELECT Id, Email, Name, Status, CreatedAt, UpdatedAt, PhoneNumber, Address, Bio, ...
// FROM Users

// ✅ FAST - Load only needed columns
var users = await context.Users
    .Select(u => new 
    {
        u.Id,
        u.Email,
        u.Name,
        OrderCount = u.Orders.Count
    })
    .ToListAsync();
// SELECT u.Id, u.Email, u.Name, COUNT(o.Id)
// FROM Users u
// LEFT JOIN Orders o ON u.Id = o.UserId
// GROUP BY u.Id, u.Email, u.Name
```

### Problem 2: Cartesian Explosion

```csharp
// ❌ Cartesian product - huge result set
var user = await context.Users
    .Include(u => u.Orders)      // 100 orders
    .Include(u => u.Addresses)   // 5 addresses
    .FirstAsync();
// Result: 100 * 5 = 500 rows (user repeated 500 times)

// ✅ Split into separate queries
var user = await context.Users
    .AsSplitQuery()
    .Include(u => u.Orders)
    .Include(u => u.Addresses)
    .FirstAsync();
// Query 1: User (1 row)
// Query 2: Orders (100 rows)
// Query 3: Addresses (5 rows)
```

### Problem 3: Function in WHERE

```csharp
// ❌ SLOW - Cannot use index
var users = await context.Users
    .Where(u => u.CreatedAt.Year == 2024)
    .ToListAsync();
// Cannot use index on CreatedAt

// ✅ FAST - Use date range
var startDate = new DateTime(2024, 1, 1);
var endDate = new DateTime(2024, 12, 31);
var users = await context.Users
    .Where(u => u.CreatedAt >= startDate && u.CreatedAt <= endDate)
    .ToListAsync();
// Can use index on CreatedAt
```

### Problem 4: Complex Filtering

```csharp
// ❌ SLOW - Multiple OR conditions
var users = await context.Users
    .Where(u => u.Status == "Active" || u.Status == "Pending" || u.Status == "Inactive")
    .ToListAsync();

// ✅ FAST - Use IN
var users = await context.Users
    .Where(u => new[] { "Active", "Pending", "Inactive" }.Contains(u.Status))
    .ToListAsync();
```

---

## Query Execution Plan

### How to View Execution Plan

**SQL Server Management Studio:**
```sql
-- Enable execution plan
Ctrl + L

SELECT u.Email, COUNT(o.Id) as OrderCount
FROM Users u
LEFT JOIN Orders o ON u.Id = o.UserId
WHERE u.Status = 'Active'
GROUP BY u.Id, u.Email
```

### Reading the Plan

```
Green checkmark = Efficient
Yellow triangle = Warning (inefficient)  
Red X = Critical problem

Common Problems:
- Table Scan → Use index
- Sort Operation → Missing index
- Spill to Disk → Query too complex
- Nested Loops → Consider join strategy
```

---

## Index Strategy

### Missing Indexes - Identify Them

```sql
SELECT 
    mid.equality_columns,
    migs.user_seeks,
    migs.avg_total_user_cost,
    migs.avg_user_impact,
    (migs.user_seeks * migs.avg_total_user_cost * migs.avg_user_impact) AS improvement
FROM sys.dm_db_missing_index_details mid
JOIN sys.dm_db_missing_index_groups mig ON mid.index_handle = mig.index_handle
JOIN sys.dm_db_missing_index_groups_stats migs ON mig.index_group_id = migs.group_handle
ORDER BY improvement DESC;
```

### Index Types

```csharp
// Single column index
public class User
{
    [Index]
    public string Email { get; set; }
}
// Good for: WHERE Email = 'x'

// Composite index (multiple columns)
public class User
{
    [Index(nameof(Status), nameof(CreatedAt))]
    public string Status { get; set; }
}
// Good for: WHERE Status = 'x' AND CreatedAt > date

// Unique index
public class User
{
    [Index(nameof(Email), IsUnique = true)]
    public string Email { get; set; }
}
// Prevents duplicates + enables faster queries

// Include columns (covering index)
modelBuilder.Entity<User>()
    .HasIndex(u => u.Email)
    .IncludeProperties(u => new { u.Name, u.Status });
// Good for: SELECT Email, Name, Status WHERE Email = 'x'
// Covers query without table lookup
```

### Fragmentation Maintenance

```sql
-- Check fragmentation
SELECT 
    OBJECT_NAME(ips.object_id) AS TableName,
    i.name AS IndexName,
    ips.avg_fragmentation_in_percent
FROM sys.dm_db_index_physical_stats(DB_ID(), NULL, NULL, NULL, 'LIMITED') ips
JOIN sys.indexes i ON ips.object_id = i.object_id 
    AND ips.index_id = i.index_id
WHERE ips.avg_fragmentation_in_percent > 10;

-- Rebuild if > 30% fragmented
ALTER INDEX IX_Users_Email ON Users REBUILD;

-- Reorganize if 10-30% fragmented
ALTER INDEX IX_Users_Email ON Users REORGANIZE;
```

---

## Batch Operations

### ❌ Slow - Individual saves

```csharp
foreach (var user in 1000users)
{
    context.Users.Add(user);
    await context.SaveChangesAsync(); // 1000 database round trips!
}
```

### ✅ Fast - Batch insert

```csharp
context.Users.AddRange(1000users);
await context.SaveChangesAsync(); // 1 database round trip
```

### Bulk Updates (EF Core 7+)

```csharp
// ❌ Slow - Load and modify
var users = await context.Users
    .Where(u => u.Status == "Pending")
    .ToListAsync();
foreach (var user in users)
{
    user.Status = "Active";
}
await context.SaveChangesAsync();

// ✅ Fast - Direct SQL
await context.Users
    .Where(u => u.Status == "Pending")
    .ExecuteUpdateAsync(s => s.SetProperty(u => u.Status, "Active"));
```

---

## Profiling Queries

### Enable Query Logging

```csharp
builder.Services.AddDbContext<EHRDbContext>(options =>
    options
        .UseSqlServer(connectionString)
        .LogTo(Console.WriteLine, LogLevel.Information)
);
```

### Find Slow Queries

```csharp
// Add stopwatch
var sw = Stopwatch.StartNew();
var users = await context.Users
    .Where(u => u.Active)
    .ToListAsync();
sw.Stop();
Console.WriteLine($"Query took {sw.ElapsedMilliseconds}ms");
```

---

## Best Practices Checklist

- [ ] Use `.AsNoTracking()` for read-only queries
- [ ] Use `.Select()` to load only needed columns
- [ ] Use `.Include()` to prevent N+1
- [ ] Use `.AsSplitQuery()` for multiple collections
- [ ] Add indexes on frequently filtered columns
- [ ] Batch operations instead of individual saves
- [ ] Use execution plans to identify slow queries
- [ ] Avoid functions in WHERE clause
- [ ] Use `.ExecuteUpdateAsync()` for bulk updates
- [ ] Monitor query performance regularly

---

## Interview Q&A

**Q: How do you optimize slow query?**

A:
1. Check execution plan for table scans
2. Add missing index on WHERE columns
3. Remove unnecessary columns (use projection)
4. Break N+1 queries (use Include)
5. Check if query is running in memory (ToList too early)

**Q: What's Cartesian explosion?**

A: When joining multiple collections, result set multiplies. Example: 1 user * 100 orders * 5 addresses = 500 rows. Solution: Use AsSplitQuery.

**Q: Why avoid functions in WHERE?**

A: 
```csharp
WHERE UPPER(Email) = 'X' // Can't use index
WHERE Email = 'x'         // Can use index
```
Index works on column as stored, not functions applied.

**Q: When to add index?**

A: When column is frequently:
- In WHERE clause
- In JOIN conditions
- In ORDER BY
