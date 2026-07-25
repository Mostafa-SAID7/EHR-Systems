# DbContext Fundamentals

## What is DbContext?

DbContext is your gateway to the database. It's a **session** that manages:
- Entity tracking
- Query execution
- Change detection
- Persisting changes

```csharp
public class EHRDbContext : DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<Patient> Patients { get; set; }
    public DbSet<Order> Orders { get; set; }
    
    public EHRDbContext(DbContextOptions options) : base(options) { }
}

// DbSet<T> = your table in C# form
// DbContext = the connection/session to database
```

---

## DbContext Lifecycle (CRITICAL!)

### Scoped Lifetime (✅ CORRECT)

```csharp
// In Program.cs
builder.Services.AddDbContext<EHRDbContext>(options =>
    options.UseSqlServer("connection-string")
);
// Default is Scoped - new instance per HTTP request

// In your service
public class UserService
{
    private readonly EHRDbContext _context;
    
    public UserService(EHRDbContext context) // New per request
    {
        _context = context;
    }
    
    public async Task<User> GetUserAsync(int id)
    {
        return await _context.Users.FirstAsync(u => u.Id == id);
    }
} // DbContext disposed after request ends
```

### Singleton Lifetime (❌ WRONG!)

```csharp
// DON'T DO THIS!
builder.Services.AddDbContext<EHRDbContext>(options =>
    options.UseSqlServer("connection-string")
);
builder.Services.AddSingleton<EHRDbContext>(); // WRONG!

// Problems:
// 1. Memory leak - DbContext never disposed
// 2. Thread-safety issues - concurrent requests share state
// 3. Stale data - old data from previous requests
// 4. Entity instances accumulate in memory
```

### Per-Request Pattern

```
Request 1              Request 2             Request 3
    ↓                      ↓                      ↓
New DbContext      New DbContext        New DbContext
    ↓                      ↓                      ↓
Query/Save         Query/Save           Query/Save
    ↓                      ↓                      ↓
Dispose            Dispose              Dispose
```

---

## OnModelCreating - Configuration

```csharp
public class EHRDbContext : DbContext
{
    public DbSet<User> Users { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configure User entity
        modelBuilder.Entity<User>(entity =>
        {
            // Table name
            entity.ToTable("Users");
            
            // Primary key
            entity.HasKey(e => e.Id);
            
            // Column configuration
            entity.Property(e => e.Email)
                .IsRequired()
                .HasMaxLength(255);
            
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");
            
            // Indexes
            entity.HasIndex(e => e.Email).IsUnique();
            entity.HasIndex(e => e.Status);
            
            // Relationships
            entity.HasMany(e => e.Orders)
                .WithOne(o => o.User)
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
```

---

## SaveChanges vs SaveChangesAsync

### Synchronous (❌ Blocks Thread)

```csharp
var user = context.Users.First(u => u.Id == 1);
user.Name = "Ahmed";
context.SaveChanges(); // Blocks thread - thread waits for database
```

### Asynchronous (✅ Non-blocking)

```csharp
var user = await context.Users.FirstAsync(u => u.Id == 1);
user.Name = "Ahmed";
await context.SaveChangesAsync(); // Thread freed while waiting
```

**Use Async in ASP.NET Core, MVC, Web APIs (multiple concurrent requests)**

```csharp
[HttpPut("{id}")]
public async Task<IActionResult> UpdateUser(int id, UpdateUserDto dto)
{
    var user = await context.Users.FirstAsync(u => u.Id == id);
    user.Email = dto.Email;
    await context.SaveChangesAsync(); // ✅ Async
    return Ok(user);
}
```

---

## Query Execution - IQueryable vs IEnumerable

```csharp
// ✅ GOOD - Executed in Database
IQueryable<User> query = context.Users
    .Where(u => u.Status == "Active")
    .OrderBy(u => u.CreatedAt);
// SQL: SELECT * FROM Users WHERE Status = 'Active' ORDER BY CreatedAt

// ❌ BAD - Executed in Memory
IEnumerable<User> users = context.Users.ToList();
var filtered = users
    .Where(u => u.Status == "Active") // Filters in memory!
    .OrderBy(u => u.CreatedAt)        // Sorts in memory!
// First loads ALL users, then filters (wasteful)
```

**Rule: Keep `.ToList()` at the END of LINQ chain**

```csharp
// ✅ Correct
var users = await context.Users
    .Where(u => u.Status == "Active")
    .OrderBy(u => u.CreatedAt)
    .ToListAsync(); // Only called last

// ❌ Wrong
var users = await context.Users.ToListAsync();
var filtered = users.Where(u => u.Status == "Active").ToList();
// Loads all users first
```

---

## DbSet - Basic Operations

```csharp
// CREATE
var newUser = new User { Email = "user@example.com" };
await context.Users.AddAsync(newUser);

// READ
var user = await context.Users.FirstAsync(u => u.Id == 1);

// UPDATE
user.Email = "newemail@example.com";
// No explicit Update() needed - EF tracks it

// DELETE
context.Users.Remove(user);

// Save all changes
await context.SaveChangesAsync();
```

---

## Multiple DbContexts

Sometimes you need multiple contexts (multi-tenant, different databases):

```csharp
public class PatientDbContext : DbContext
{
    public DbSet<Patient> Patients { get; set; }
}

public class BillingDbContext : DbContext
{
    public DbSet<Invoice> Invoices { get; set; }
}

// Register both
builder.Services.AddDbContext<PatientDbContext>();
builder.Services.AddDbContext<BillingDbContext>();

// Use in service
public class OrderService
{
    private readonly PatientDbContext _patientContext;
    private readonly BillingDbContext _billingContext;
    
    public OrderService(PatientDbContext patientContext, 
                       BillingDbContext billingContext)
    {
        _patientContext = patientContext;
        _billingContext = billingContext;
    }
}
```

---

## Interview Q&A

**Q: Why is DbContext scoped and not singleton?**

A: Because:
- DbContext tracks entity state (memory overhead)
- Thread-safety issues with concurrent requests
- Each request needs isolated state
- Prevents memory leaks and stale data

**Q: When should you use SaveChanges vs SaveChangesAsync?**

A: 
- SaveChanges: Console apps, background jobs (single-threaded)
- SaveChangesAsync: ASP.NET Core, web APIs (multi-threaded)

**Q: Difference between FirstAsync and SingleAsync?**

A:
- `FirstAsync(predicate)` - Returns first match, throws if none
- `SingleAsync(predicate)` - Returns exactly one match, throws if 0 or 2+

```csharp
var first = await context.Users.FirstAsync(u => u.Id == 1); // OK
var single = await context.Users.SingleAsync(u => u.Id == 1); // OK if exactly 1
```

**Q: What's the difference between DbSet and DbContext?**

A:
- DbContext: Session/connection to database
- DbSet: Represents a table within that context
