# IEnumerable, IQueryable, and LINQ Execution

## IEnumerable (LINQ to Objects)

**Definition:** In-memory collection enumeration.

```csharp
List<User> users = new()
{
    new User { Id = 1, Name = "Ahmed", Age = 30 },
    new User { Id = 2, Name = "Ali", Age = 25 },
    new User { Id = 3, Name = "Sara", Age = 28 }
};

// LINQ to Objects (IEnumerable)
IEnumerable<User> result = users
    .Where(u => u.Age > 25)
    .OrderBy(u => u.Name)
    .Select(u => new { u.Name, u.Age });

// Executed in-memory with LINQ to Objects
foreach (var user in result)
{
    Console.WriteLine($"{user.Name}: {user.Age}");
}
```

**How it works:**
- Loads entire collection into memory
- Filters/transforms using LINQ-to-Objects delegates
- `Where` → loops through each item → checks condition

---

## IQueryable (LINQ to Providers)

**Definition:** Query provider pattern - translates to external query language (SQL).

```csharp
using var context = new DbContext();

// LINQ to Entity Framework (IQueryable)
IQueryable<User> query = context.Users
    .Where(u => u.Age > 25)
    .OrderBy(u => u.Name)
    .Select(u => new { u.Name, u.Age });

// Executed on database server!
List<dynamic> result = await query.ToListAsync();
```

**What actually happens:**
```
LINQ Query → Expression Tree → SQL Translation → Sent to Database
```

---

## Key Differences

| Feature | IEnumerable | IQueryable |
|---------|-------------|-----------|
| Location | In-memory | External (DB) |
| Execution | LINQ-to-Objects | Query provider |
| When Executed | Deferred | Deferred |
| Filter Push-down | No | Yes (to DB) |
| SQL Generation | No | Yes |
| Method Provider | System.Linq | System.Linq.Queryable |
| Extension Methods | IEnumerable.Where() | IQueryable.Where() |

---

## Deferred vs Immediate Execution

```csharp
var users = GetUsersFromDatabase();

// DEFERRED - Query not executed yet
IEnumerable<User> query = users.Where(u => u.Age > 25);
// Just defines the query, doesn't run

// IMMEDIATE - Query executes now
List<User> result = users.Where(u => u.Age > 25).ToList();
// Actually filters and returns results

// Immediate triggers deferred
foreach (var user in query) // Executes here!
{
    Console.WriteLine(user.Name);
}
```

---

## Dangerous: The N+1 Problem

```csharp
// ❌ BAD - N+1 queries (1 parent + N children)
var users = context.Users; // Query 1
foreach (var user in users)
{
    var orders = user.Orders; // Query for each user (N queries!)
    Console.WriteLine($"{user.Name}: {orders.Count}");
}
// Result: 1 + N database queries!

// ✅ GOOD - Eager loading (single optimized query)
var users = await context.Users
    .Include(u => u.Orders) // Load orders in single query
    .ToListAsync();
foreach (var user in users)
{
    Console.WriteLine($"{user.Name}: {user.Orders.Count}");
}
// Result: 1 database query
```

---

## LINQ Execution Strategies

```csharp
using var context = new DbContext();

// 1. IMMEDIATE EXECUTION
var count = context.Users.Count(); // Executes immediately
var list = context.Users.ToList(); // Executes immediately

// 2. DEFERRED EXECUTION
IQueryable<User> query = context.Users.Where(u => u.Age > 25);
// Not executed yet!

// Executes when enumerated
foreach (var user in query)
{
    // Execution happens here
}

// 3. EXPLICIT EXECUTION
var materialize = context.Users
    .Where(u => u.Age > 25)
    .ToListAsync(); // Explicitly materialize to List
```

---

## AsNoTracking() Performance

```csharp
// ❌ Default - Tracks changes (slower)
var users = await context.Users
    .Where(u => u.Age > 25)
    .ToListAsync();

// ✅ Read-only - No tracking (faster)
var users = await context.Users
    .AsNoTracking()
    .Where(u => u.Age > 25)
    .ToListAsync();

// Use AsNoTracking() when:
// - Not modifying entities
// - Reading for display
// - Large result sets
// - Performance-critical queries
```

---

## AsSplitQuery() - Fixing Cartesian Explosion

```csharp
// ❌ BAD - Cartesian product with multiple Include
var users = await context.Users
    .Include(u => u.Orders)
    .Include(u => u.Addresses)
    .ToListAsync();
// If user has 5 orders and 3 addresses: returns 5*3=15 rows!

// ✅ GOOD - Multiple queries, no explosion
var users = await context.Users
    .AsSplitQuery()
    .Include(u => u.Orders)
    .Include(u => u.Addresses)
    .ToListAsync();
// Returns correct count: 1 user with 5 orders and 3 addresses
```

---

## Interview Q&A

**Q: IEnumerable vs IQueryable - when to use which?**

A:
```csharp
// Use IEnumerable for:
// - Already filtered data in memory
// - Simple LINQ-to-Objects operations
List<User> users = GetUsersFromFile();
var results = users.Where(u => u.Age > 25).ToList();

// Use IQueryable for:
// - Database queries (push filtering to DB)
// - Large datasets (don't load everything)
var results = await context.Users
    .Where(u => u.Age > 25)
    .ToListAsync();
```

**Q: Why is N+1 problem dangerous?**

A: If you have 100 users and load their orders without `Include()`:
- Query 1: Get 100 users
- Queries 2-101: Get orders for each user
- Total: 101 queries instead of 2!

**Q: When should I use ToList() vs AsEnumerable()?**

A:
```csharp
// ToList() - Materialize to list (execute query)
var list = query.ToList();

// AsEnumerable() - Just change type to IEnumerable
var enumerable = query.AsEnumerable();

// Use ToList() when you need a collection
// Use AsEnumerable() to switch from IQueryable to LINQ-to-Objects
```
