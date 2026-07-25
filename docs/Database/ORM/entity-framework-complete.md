# Entity Framework Complete Guide

## DbContext - The Heart of EF

```csharp
public class EHRDbContext : DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<Patient> Patients { get; set; }
    public DbSet<Order> Orders { get; set; }
    
    public EHRDbContext(DbContextOptions<EHRDbContext> options) : base(options) { }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configure entities
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
            entity.HasIndex(e => e.Email).IsUnique();
        });
    }
}
```

---

## Fluent API Configuration

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    // User configuration
    modelBuilder.Entity<User>(entity =>
    {
        entity.ToTable("Users");
        entity.HasKey(e => e.Id);
        
        entity.Property(e => e.Email)
            .IsRequired()
            .HasMaxLength(255);
            
        entity.Property(e => e.CreatedAt)
            .HasDefaultValueSql("GETUTCDATE()");
            
        entity.HasIndex(e => e.Email).IsUnique();
        entity.HasIndex(e => e.Status);
        
        entity.HasMany(e => e.Orders)
            .WithOne(o => o.User)
            .HasForeignKey(o => o.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    });
    
    // Relationships
    modelBuilder.Entity<Order>()
        .HasOne(o => o.User)
        .WithMany(u => u.Orders)
        .HasForeignKey(o => o.UserId);
}
```

---

## Change Tracking Deep Dive

```csharp
var user = context.Users.First(u => u.Id == 1);
// State: Unchanged (EF created snapshot)

user.Name = "Ahmed";
// State: Modified (EF detected change)

var newUser = new User { Name = "Ali" };
context.Users.Add(newUser);
// State: Added

context.SaveChanges();
// Generates: UPDATE Users SET Name = 'Ahmed' WHERE Id = 1;
//           INSERT INTO Users VALUES (...)

// Track all entities
var entries = context.ChangeTracker.Entries();
foreach (var entry in entries)
{
    Console.WriteLine($"{entry.Entity} - {entry.State}");
}
```

---

## AsNoTracking Performance

```csharp
// ❌ WITH TRACKING (Default)
var users = context.Users
    .Where(u => u.Active)
    .ToList();
// Stores snapshot of each user in memory

// ✅ WITHOUT TRACKING
var users = context.Users
    .AsNoTracking()
    .Where(u => u.Active)
    .ToList();
// No snapshots, faster for read-only

// Use AsNoTracking when:
// - No modifications planned
// - Large result sets
// - Display/report queries
```

---

## Loading Patterns

### Lazy Loading (❌ N+1)

```csharp
var user = context.Users.First(u => u.Id == 1);
var orderCount = user.Orders.Count;  // Separate query!
// Query 1: SELECT * FROM Users WHERE Id = 1
// Query 2: SELECT * FROM Orders WHERE UserId = 1
```

### Eager Loading (✅ Optimized)

```csharp
var user = context.Users
    .Include(u => u.Orders)
    .First(u => u.Id == 1);
var orderCount = user.Orders.Count; // No additional query
// Single query with JOIN
```

### Explicit Loading (Manual)

```csharp
var user = context.Users.First(u => u.Id == 1);
// Later, load orders
await context.Entry(user)
    .Collection(u => u.Orders)
    .LoadAsync();
```

---

## Pagination Implementation

```csharp
public async Task<PaginatedResult<User>> GetUsersAsync(int pageNumber, int pageSize)
{
    var query = context.Users.AsQueryable();
    
    var total = await query.CountAsync();
    
    var items = await query
        .Skip((pageNumber - 1) * pageSize)
        .Take(pageSize)
        .ToListAsync();
    
    return new PaginatedResult<User>
    {
        Items = items,
        Total = total,
        PageNumber = pageNumber,
        PageSize = pageSize,
        TotalPages = (total + pageSize - 1) / pageSize
    };
}
```

---

## Migrations - Schema Versioning

```bash
# Create migration
dotnet ef migrations add InitialCreate

# List migrations
dotnet ef migrations list

# Apply migrations
dotnet ef database update

# Revert to previous
dotnet ef database update PreviousMigrationName

# Remove last migration (not applied)
dotnet ef migrations remove

# Generate SQL script
dotnet ef migrations script
```

---

## Transactions & Concurrency

### Transaction

```csharp
using var transaction = await context.Database.BeginTransactionAsync();
try
{
    var user = await context.Users.FirstAsync(u => u.Id == 1);
    user.Balance -= 100;
    
    var account = await context.Accounts.FirstAsync(a => a.Id == 1);
    account.Balance += 100;
    
    await context.SaveChangesAsync();
    await transaction.CommitAsync();
}
catch
{
    await transaction.RollbackAsync();
    throw;
}
```

### Optimistic Locking

```csharp
public class User
{
    public int Id { get; set; }
    public string Name { get; set; }
    
    [Timestamp]
    public byte[] RowVersion { get; set; }
}

// Usage
try
{
    var user = context.Users.First(u => u.Id == 1);
    user.Name = "New Name";
    context.SaveChanges(); // Checks RowVersion
}
catch (DbUpdateConcurrencyException)
{
    // Another user modified this record
}
```

---

## Query Optimization

### AsSplitQuery - Fix Cartesian Explosion

```csharp
// ❌ BAD - Cartesian product
var user = context.Users
    .Include(u => u.Orders)      // 5 orders
    .Include(u => u.Addresses)   // 3 addresses
    .First();
// Returns 5*3 = 15 rows (duplicated data)

// ✅ GOOD - Multiple queries
var user = context.Users
    .AsSplitQuery()
    .Include(u => u.Orders)
    .Include(u => u.Addresses)
    .First();
// Query 1: User, Query 2: Orders, Query 3: Addresses
```

### Projection - Load Only Needed Fields

```csharp
// ❌ Full entity (all fields)
var users = context.Users.ToList();
// Loads every column

// ✅ Projection (only needed fields)
var users = context.Users
    .Select(u => new { u.Id, u.Email, u.Name })
    .ToList();
// Loads only 3 columns, faster
```

---

## Indexes & Performance

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<User>(entity =>
    {
        // Single column index
        entity.HasIndex(e => e.Email).IsUnique();
        
        // Composite index
        entity.HasIndex(e => new { e.Status, e.CreatedAt });
        
        // Include columns (cover index)
        entity.HasIndex(e => e.Email)
            .IncludeProperties(e => new { e.Name, e.Status });
    });
}
```

---

## Interview Q&A

**Q: What's the difference between tracking and no-tracking?**

A: Tracking stores snapshots of entities to detect changes. NoTracking skips this overhead for read-only queries, improving performance.

**Q: How do you prevent N+1 queries?**

A: Use Include() for eager loading or Select() for projection instead of accessing navigation properties directly.

**Q: When should you use SaveChanges vs SaveChangesAsync?**

A: Async for I/O operations (database, network). Sync blocks thread, use in console apps only.

**Q: What's optimistic vs pessimistic locking?**

A: 
- Optimistic: Check version on save, retry if conflict (best for web)
- Pessimistic: Lock row during edit (blocks concurrent access)
