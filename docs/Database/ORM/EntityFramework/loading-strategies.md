# Loading Strategies - Lazy, Eager, Explicit

## The Problem: N+1 Query

```csharp
var users = context.Users.ToList(); // Query 1
// Result: 100 users

foreach (var user in users)
{
    var count = user.Orders.Count; // Query 2-101 (one per user!)
    // Each access triggers separate query
}
// Total: 101 queries = SLOW
```

---

## Solution 1: Eager Loading (Include)

**Load related data upfront in single query.**

### Single Related Entity

```csharp
var user = await context.Users
    .Include(u => u.Orders) // Load Orders eagerly
    .FirstAsync(u => u.Id == 1);

var orderCount = user.Orders.Count; // No additional query
// Total: 1 query
```

### Multiple Related Entities

```csharp
var user = await context.Users
    .Include(u => u.Orders)
    .Include(u => u.Addresses)
    .Include(u => u.Notifications)
    .FirstAsync(u => u.Id == 1);
// Loads: User + Orders + Addresses + Notifications in one query
```

### Nested Relationships (ThenInclude)

```csharp
var user = await context.Users
    .Include(u => u.Orders)
        .ThenInclude(o => o.Items) // Load Order Items
        .ThenInclude(i => i.Product) // Load Product details
    .Include(u => u.Addresses)
    .FirstAsync(u => u.Id == 1);
// Loads: User → Orders → OrderItems → Products
```

### Filtering Included Data

```csharp
// ⚠️ Note: Cannot filter Include directly

// ❌ This doesn't work:
var user = await context.Users
    .Include(u => u.Orders.Where(o => o.Status == "Completed"))
    .FirstAsync();

// ✅ Use separate query instead:
var user = await context.Users
    .FirstAsync(u => u.Id == 1);
var completedOrders = await context.Orders
    .Where(o => o.UserId == 1 && o.Status == "Completed")
    .ToListAsync();
user.Orders = completedOrders;
```

---

## Solution 2: Explicit Loading (LoadAsync)

**Load related data later when needed.**

### Basic Explicit Loading

```csharp
var user = await context.Users.FirstAsync(u => u.Id == 1);
// Orders NOT loaded yet

// Later, explicitly load
await context.Entry(user)
    .Collection(u => u.Orders)
    .LoadAsync();
// Now Orders available
var orderCount = user.Orders.Count;
```

### Conditional Loading

```csharp
var user = await context.Users.FirstAsync(u => u.Id == 1);

// Only load orders if needed
if (needOrders)
{
    await context.Entry(user)
        .Collection(u => u.Orders)
        .LoadAsync();
}

// Only load addresses if needed
if (needAddresses)
{
    await context.Entry(user)
        .Collection(u => u.Addresses)
        .LoadAsync();
}
```

### Reference Property (Single Item)

```csharp
var order = await context.Orders.FirstAsync();
// User NOT loaded

// Load related user
await context.Entry(order)
    .Reference(o => o.User)
    .LoadAsync();

Console.WriteLine(order.User.Name); // Now available
```

---

## Solution 3: Lazy Loading (❌ Avoid)

**Related data loaded automatically when accessed.**

Requires virtual properties:

```csharp
public class User
{
    public int Id { get; set; }
    public virtual ICollection<Order> Orders { get; set; } // virtual keyword
}

var user = context.Users.First(u => u.Id == 1);
var orderCount = user.Orders.Count; // Lazy loaded - separate query!
```

**Problems:**
- N+1 query problem
- Hard to debug (hidden queries)
- Can't control query strategy
- Not async-friendly

---

## AsSplitQuery - Fix Cartesian Explosion

When including multiple collections, result set can explode:

```csharp
// ❌ Cartesian product (too many rows)
var user = await context.Users
    .Include(u => u.Orders)      // 5 orders
    .Include(u => u.Addresses)   // 3 addresses
    .FirstAsync(u => u.Id == 1);

// Database returns: 1 user * 5 orders * 3 addresses = 15 rows
// User data duplicated 15 times!
// Memory waste and slower transfer

// ✅ Split into separate queries
var user = await context.Users
    .AsSplitQuery()
    .Include(u => u.Orders)
    .Include(u => u.Addresses)
    .FirstAsync(u => u.Id == 1);

// Executes: 3 queries instead
// Query 1: User (1 row)
// Query 2: User's Orders (5 rows)
// Query 3: User's Addresses (3 rows)
// No duplication!
```

---

## Projection (Select) - Best Performance

**Only load fields you need.**

```csharp
// ❌ Load all fields
var users = await context.Users.ToListAsync();
// Loads: Id, Email, Name, Status, CreatedAt, UpdatedAt, PhoneNumber, Address...

// ✅ Load only needed fields
var users = await context.Users
    .Select(u => new 
    {
        u.Id,
        u.Email,
        u.Name,
        OrderCount = u.Orders.Count
    })
    .ToListAsync();
// Loads only 4 columns + calculated OrderCount
```

With DTO:

```csharp
public class UserListDto
{
    public int Id { get; set; }
    public string Email { get; set; }
    public string Name { get; set; }
    public int OrderCount { get; set; }
}

var users = await context.Users
    .Select(u => new UserListDto
    {
        Id = u.Id,
        Email = u.Email,
        Name = u.Name,
        OrderCount = u.Orders.Count
    })
    .ToListAsync();
```

---

## Comparison Table

| Strategy | Query Count | Memory | Use Case |
|----------|-------------|--------|----------|
| Lazy | N+1 | High | ❌ Avoid |
| Eager (Include) | 1-2 | Medium | ✅ Most common |
| Explicit | 1+ | Medium | ✅ Conditional |
| Projection | 1 | Low | ✅ Best performance |
| AsSplitQuery | 2+ | Low | ✅ Multiple collections |

---

## Best Practices

```csharp
// ✅ GOOD - Eager load what you need, project only needed fields
var users = await context.Users
    .AsNoTracking()
    .Include(u => u.Orders) // Load related data
    .Select(u => new UserDto // Project to DTO
    {
        Id = u.Id,
        Email = u.Email,
        OrderCount = u.Orders.Count
    })
    .ToListAsync();

// ❌ BAD - Lazy loading (N+1)
var users = await context.Users.ToListAsync();
foreach (var user in users)
{
    var orders = user.Orders; // N+1
}
```

---

## Interview Q&A

**Q: What's N+1 query problem?**

A: 1 query to load users + N queries to load their orders = N+1 total. Caused by lazy loading. Fix: Use Include() or Explicit Loading.

**Q: Include vs ThenInclude?**

A:
- Include: Load one level of related data
- ThenInclude: Load nested level after Include

**Q: When to use AsNoTracking?**

A: With read-only queries, large result sets, and displays. Skip if modifying data.

**Q: Eager vs Explicit loading?**

A:
- Eager (Include): Load upfront, best for most cases
- Explicit: Load conditionally later, when you don't know if needed

**Q: Why use Projection?**

A: Loads only needed columns, reduces memory and transfer time. Best for reports/dashboards.
