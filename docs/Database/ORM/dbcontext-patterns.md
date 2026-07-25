# DbContext Patterns & Best Practices

## DbContext Lifecycle

```csharp
// DbContext should be short-lived (per request)
using (var context = new EHRDbContext(options))
{
    // Use context
    var user = context.Users.First();
} // Automatically disposed
```

---

## Generic Repository with DbContext

```csharp
public interface IRepository<T> where T : class
{
    Task<T> GetByIdAsync(int id);
    Task<List<T>> GetAllAsync();
    Task AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(T entity);
}

public class Repository<T> : IRepository<T> where T : class
{
    private readonly DbContext _context;
    private readonly DbSet<T> _dbSet;
    
    public Repository(DbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }
    
    public async Task<T> GetByIdAsync(int id)
    {
        return await _dbSet.FindAsync(id);
    }
    
    public async Task<List<T>> GetAllAsync()
    {
        return await _dbSet.ToListAsync();
    }
    
    public async Task AddAsync(T entity)
    {
        await _dbSet.AddAsync(entity);
    }
    
    public async Task UpdateAsync(T entity)
    {
        _dbSet.Update(entity);
    }
    
    public async Task DeleteAsync(T entity)
    {
        _dbSet.Remove(entity);
    }
}
```

---

## Unit of Work with DbContext

```csharp
public interface IUnitOfWork : IDisposable
{
    IRepository<User> Users { get; }
    IRepository<Order> Orders { get; }
    Task<int> SaveChangesAsync();
}

public class UnitOfWork : IUnitOfWork
{
    private readonly DbContext _context;
    private IRepository<User> _userRepository;
    private IRepository<Order> _orderRepository;
    
    public UnitOfWork(DbContext context)
    {
        _context = context;
    }
    
    public IRepository<User> Users =>
        _userRepository ??= new Repository<User>(_context);
    
    public IRepository<Order> Orders =>
        _orderRepository ??= new Repository<Order>(_context);
    
    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }
    
    public void Dispose()
    {
        _context?.Dispose();
    }
}
```

---

## Usage in Service

```csharp
public class OrderService
{
    private readonly IUnitOfWork _unitOfWork;
    
    public async Task<Order> CreateOrderAsync(Order order)
    {
        // Add order
        await _unitOfWork.Orders.AddAsync(order);
        
        // Update user
        var user = await _unitOfWork.Users.GetByIdAsync(order.UserId);
        user.OrderCount++;
        await _unitOfWork.Users.UpdateAsync(user);
        
        // Save all changes together
        await _unitOfWork.SaveChangesAsync();
        
        return order;
    }
}
```

---

## Context Configuration

```csharp
// Program.cs - Setup DbContext
builder.Services.AddDbContext<EHRDbContext>(options =>
    options
        .UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
        .EnableSensitiveDataLogging(app.Environment.IsDevelopment())
        .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking) // Default to NoTracking
);

// OR for testing
builder.Services.AddDbContext<EHRDbContext>(options =>
    options.UseInMemoryDatabase("TestDb")
);
```

---

## Query Caching

```csharp
public class CachedUserRepository : IUserRepository
{
    private readonly IRepository<User> _repository;
    private readonly IMemoryCache _cache;
    
    public async Task<User> GetByIdAsync(int id)
    {
        var cacheKey = $"user_{id}";
        
        if (_cache.TryGetValue(cacheKey, out User cachedUser))
            return cachedUser;
        
        var user = await _repository.GetByIdAsync(id);
        
        _cache.Set(cacheKey, user, TimeSpan.FromMinutes(10));
        
        return user;
    }
}
```

---

## Error Handling

```csharp
public async Task<bool> CreateUserAsync(User user)
{
    try
    {
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();
        return true;
    }
    catch (DbUpdateException ex) when (ex.InnerException is SqlException sqlEx && sqlEx.Number == 2627)
    {
        // Unique constraint violation
        throw new DuplicateUserException("Email already exists");
    }
    catch (DbUpdateException ex)
    {
        // Database error
        throw new DataAccessException("Failed to create user", ex);
    }
    catch (OperationCanceledException)
    {
        // Timeout
        throw new TimeoutException("Database operation timed out");
    }
}
```

---

## Bulk Operations

```csharp
// ❌ Slow - Individual saves
foreach (var user in users)
{
    context.Users.Add(user);
    await context.SaveChangesAsync(); // Save each one
}

// ✅ Fast - Batch insert
context.Users.AddRange(users);
await context.SaveChangesAsync(); // Single save
```

---

## Interview Q&A

**Q: Should DbContext be singleton?**

A: No! DbContext should be scoped (per request). Singleton causes:
- Memory leaks
- Thread-safety issues
- Stale data tracking

**Q: How to handle DbContext disposal?**

A: Use `using` statement or DI container handles it automatically with AddScoped.

**Q: Best practice for multi-tenant?**

A: Use separate DbContext per tenant or filter queries by TenantId.
