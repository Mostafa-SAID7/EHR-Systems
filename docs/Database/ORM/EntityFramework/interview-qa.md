# Entity Framework - Complete Interview Q&A

## Fundamental Questions

### Q1: What is Entity Framework and why use it?

**A:** Entity Framework is an ORM (Object-Relational Mapper) that bridges C# objects and databases.

**Why use it:**
- Type-safe LINQ queries (compile-time checking)
- Automatic change tracking
- Database migrations (version control for schema)
- Works across multiple databases (SQL Server, MySQL, PostgreSQL)
- Built-in async/await support
- Relationship handling (foreign keys, navigation properties)

**Trade-off:** Slower than raw SQL for complex queries.

---

### Q2: DbContext - what is it and why scoped?

**A:** DbContext is your session/connection to the database. It manages:
- Entity tracking (snapshots)
- Query execution
- Change detection
- Persisting changes

**Why Scoped?**
- New instance per HTTP request
- Each request has isolated entity state
- Disposed after request ends
- Prevents memory leaks and stale data

**Wrong:** Singleton DbContext causes thread-safety issues and memory leaks.

---

### Q3: What are the 5 entity states?

**A:**
```csharp
EntityState.Detached    // Not tracked
EntityState.Added       // New, will be inserted
EntityState.Unchanged   // Loaded, no changes
EntityState.Modified    // Loaded, has changes
EntityState.Deleted     // Marked for deletion
```

---

### Q4: How does change tracking work?

**A:**
1. Load entity → EF creates snapshot
2. Modify properties → EF compares with snapshot
3. SaveChanges → EF generates SQL for changed properties

**Example:**
```csharp
var user = context.Users.First(); // Snapshot: { Email: "old@test" }
user.Email = "new@test";
await context.SaveChangesAsync();
// UPDATE Users SET Email = 'new@test' WHERE Id = 1
```

---

### Q5: Tracking vs NoTracking - when to use each?

**Tracking (Default):**
- Use when: Modifying data, need change detection
- Cost: Memory (snapshots), CPU (comparison)

**NoTracking:**
- Use when: Read-only queries, reporting, large result sets
- Benefit: Faster, less memory, no overhead

```csharp
// Modify
var user = context.Users.First(); // Tracking
user.Name = "New";
await context.SaveChangesAsync();

// Display
var users = context.Users.AsNoTracking().ToList(); // No tracking
```

---

## Loading Strategies

### Q6: What's the N+1 query problem?

**A:** 1 query to load users + N queries to load each user's orders = N+1 total.

```csharp
// ❌ N+1
var users = context.Users.ToList(); // Query 1
foreach (var user in users)
{
    var orders = user.Orders.Count; // Queries 2-N (1 per user)
}

// ✅ Solution: Include
var users = context.Users
    .Include(u => u.Orders)
    .ToList(); // Query 1 (with JOIN)
```

---

### Q7: Include vs ThenInclude vs Select?

**A:**
```csharp
// Include - Load one level
var users = context.Users
    .Include(u => u.Orders)
    .ToList();

// ThenInclude - Nested loading
var users = context.Users
    .Include(u => u.Orders)
        .ThenInclude(o => o.Items) // Nested
    .ToList();

// Select - Load only needed fields (best performance)
var users = context.Users
    .Select(u => new { u.Id, u.Email })
    .ToList();
```

---

### Q8: Lazy Loading - why avoid?

**A:** Lazy loading automatically loads related data when accessed, causing N+1.

```csharp
// ❌ Lazy loading
var user = context.Users.First();
var orders = user.Orders.Count; // Separate query!

// ✅ Avoid by using Include upfront
var user = context.Users
    .Include(u => u.Orders)
    .First();
var orders = user.Orders.Count; // No query
```

---

### Q9: Explicit Loading - when to use?

**A:** Load related data later, conditionally.

```csharp
var user = context.Users.First();

if (needOrders)
{
    await context.Entry(user)
        .Collection(u => u.Orders)
        .LoadAsync();
}
```

---

### Q10: What's AsSplitQuery?

**A:** Splits query into multiple queries to avoid Cartesian explosion.

```csharp
// ❌ Cartesian (1 user * 100 orders * 5 addresses = 500 rows)
var user = context.Users
    .Include(u => u.Orders)
    .Include(u => u.Addresses)
    .First();

// ✅ Split
var user = context.Users
    .AsSplitQuery()
    .Include(u => u.Orders)
    .Include(u => u.Addresses)
    .First();
// Query 1: User, Query 2: Orders, Query 3: Addresses
```

---

## Query Optimization

### Q11: What's execution plan and why important?

**A:** SQL Server's strategy to execute your query. Shows:
- Table scans vs index seeks
- JOIN strategies
- Bottlenecks

**Green checkmark** = efficient
**Yellow triangle** = warning
**Red X** = critical

---

### Q12: When to add index?

**A:** Add index when column is frequently:
- In WHERE clause
- In JOIN condition
- In ORDER BY

```csharp
[Index(nameof(Email))]          // Single column
[Index(nameof(Status), nameof(CreatedAt))] // Composite
[Index(nameof(Email), IsUnique = true)]    // Unique
```

---

### Q13: Why not use functions in WHERE?

**A:** Functions prevent index usage.

```csharp
// ❌ Can't use index
WHERE UPPER(Email) = 'X'
WHERE YEAR(CreatedAt) = 2024

// ✅ Can use index
WHERE Email = 'x'
WHERE CreatedAt >= '2024-01-01' AND CreatedAt < '2025-01-01'
```

---

### Q14: Projection (Select) - why important?

**A:** Load only needed columns, reduce memory and transfer.

```csharp
// ❌ All columns
var users = context.Users.ToList();

// ✅ Only needed
var users = context.Users
    .Select(u => new { u.Id, u.Email })
    .ToList();
```

---

## Migrations

### Q15: What's a migration?

**A:** Version-controlled snapshot of database schema.

```bash
dotnet ef migrations add AddPhoneColumn
# Generates migration file with Up() and Down()

dotnet ef database update
# Applies migration (Up method)

dotnet ef database update PreviousMigration
# Rollback (Down method)
```

---

### Q16: How to handle migrations in production?

**A:**
1. Generate SQL script: `dotnet ef migrations script`
2. Review SQL for safety
3. Apply to production
4. Never run directly on production without review

---

## Transactions & Concurrency

### Q17: What's a transaction?

**A:** Group of operations that succeed together or fail together (ACID).

```csharp
using var transaction = await context.Database.BeginTransactionAsync();
try
{
    // Multiple operations
    await context.SaveChangesAsync();
    await transaction.CommitAsync();
}
catch
{
    await transaction.RollbackAsync();
}
```

---

### Q18: Optimistic vs Pessimistic Locking?

**A:**
- **Optimistic:** Assume conflicts rare, check at save (use RowVersion)
- **Pessimistic:** Lock row during edit, blocks access

```csharp
// Optimistic
[Timestamp]
public byte[] RowVersion { get; set; }

// Pessimistic
using var transaction = await context.Database.BeginTransactionAsync(
    System.Data.IsolationLevel.Serializable);
```

---

### Q19: How to handle concurrency exception?

**A:**
```csharp
catch (DbUpdateConcurrencyException ex)
{
    var databaseValues = await ex.Entries[0].GetDatabaseValuesAsync();
    // Option 1: Keep your changes
    await ex.Entries[0].ReloadAsync();
    user.Email = newEmail;
    await context.SaveChangesAsync();
    
    // Option 2: Alert user
    throw new OptimisticLockException("Record was modified");
}
```

---

## Performance & Best Practices

### Q20: SaveChanges vs SaveChangesAsync?

**A:**
- **SaveChanges:** Blocks thread, use in console apps
- **SaveChangesAsync:** Non-blocking, use in ASP.NET Core/APIs

---

### Q21: Common performance mistakes?

**A:**
1. N+1 queries (missing Include)
2. Loading all columns (missing Select)
3. Large result sets with tracking
4. Functions in WHERE clause
5. Individual saves in loop (use batch)

---

### Q22: How to prevent N+1?

**A:**
- Use Include() for eager loading
- Use Select() for projection
- Use AsSplitQuery() for multiple collections
- Avoid lazy loading (virtual properties)

---

## Architecture Patterns

### Q23: Repository Pattern - when to use?

**A:** When you want to:
- Abstract DbContext
- Centralize data access logic
- Make testing easier (mock repository)

---

### Q24: Unit of Work Pattern?

**A:** Coordinates multiple repositories, single SaveChanges.

```csharp
public class OrderService
{
    private readonly IUnitOfWork _unitOfWork;
    
    public async Task CreateOrderAsync(Order order)
    {
        await _unitOfWork.Orders.AddAsync(order);
        var user = await _unitOfWork.Users.GetByIdAsync(order.UserId);
        user.TotalOrders++;
        await _unitOfWork.Users.UpdateAsync(user);
        
        await _unitOfWork.SaveChangesAsync(); // Single save
    }
}
```

---

### Q25: CQRS Pattern benefits?

**A:**
- Separates read and write models
- Optimize each independently
- Can use read replicas
- Better for complex applications

---

## Tricky Questions

### Q26: DbContext as singleton - good or bad?

**A:** **BAD!** Causes:
- Memory leaks (never disposed)
- Thread-safety issues
- Stale data
- Entity state accumulation

**Always use Scoped.**

---

### Q27: What happens if SaveChanges called twice?

**A:**
```csharp
user.Email = "email1@test.com";
await context.SaveChangesAsync(); // UPDATE 1

user.Email = "email2@test.com";
await context.SaveChangesAsync(); // UPDATE 2 (with email2)
```

---

### Q28: ToList() vs ToListAsync()?

**A:**
- **ToList():** Synchronous, blocks thread
- **ToListAsync():** Asynchronous, non-blocking

Use Async in web apps.

---

### Q29: What's the cost of change tracking?

**A:**
- Memory: Snapshot per entity (~100 bytes each)
- CPU: Comparison on SaveChanges
- For 10,000 entities: ~1-2MB + CPU overhead

Solution: Use AsNoTracking() for read-only.

---

### Q30: How to optimize bulk insert?

**A:**
```csharp
// ❌ Slow
foreach (var user in 1000users)
{
    context.Users.Add(user);
    await context.SaveChangesAsync();
}

// ✅ Fast
context.Users.AddRange(1000users);
await context.SaveChangesAsync();
```

---

## Final Tips

1. **Always use AsNoTracking()** for read-only queries
2. **Always use Include()** to prevent N+1
3. **Always use Select()** to load only needed fields
4. **Never use DbContext as Singleton**
5. **Always test execution plans** for slow queries
6. **Always batch operations** instead of looping saves
7. **Always use async** in web applications
