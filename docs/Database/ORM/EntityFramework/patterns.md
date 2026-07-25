# Architecture Patterns with EF

## Repository Pattern

**Goal:** Abstract DbContext, provide clean data access interface.

```csharp
public interface IRepository<T> where T : class
{
    Task<T> GetByIdAsync(int id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
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
    
    public async Task<T> GetByIdAsync(int id) => 
        await _dbSet.FindAsync(id);
    
    public async Task<IEnumerable<T>> GetAllAsync() => 
        await _dbSet.ToListAsync();
    
    public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate) => 
        await _dbSet.Where(predicate).ToListAsync();
    
    public async Task AddAsync(T entity) => 
        await _dbSet.AddAsync(entity);
    
    public async Task UpdateAsync(T entity) => 
        _dbSet.Update(entity);
    
    public async Task DeleteAsync(T entity) => 
        _dbSet.Remove(entity);
}
```

### Benefits

- ✅ Decouple business logic from DbContext
- ✅ Easy to mock for testing
- ✅ Single place to modify data access
- ✅ Consistent querying patterns

---

## Unit of Work Pattern

**Goal:** Coordinate multiple repositories, single SaveChanges.

```csharp
public interface IUnitOfWork : IDisposable
{
    IRepository<User> Users { get; }
    IRepository<Order> Orders { get; }
    IRepository<Patient> Patients { get; }
    Task<int> SaveChangesAsync();
}

public class UnitOfWork : IUnitOfWork
{
    private readonly DbContext _context;
    
    private IRepository<User> _userRepository;
    private IRepository<Order> _orderRepository;
    private IRepository<Patient> _patientRepository;
    
    public UnitOfWork(DbContext context)
    {
        _context = context;
    }
    
    public IRepository<User> Users =>
        _userRepository ??= new Repository<User>(_context);
    
    public IRepository<Order> Orders =>
        _orderRepository ??= new Repository<Order>(_context);
    
    public IRepository<Patient> Patients =>
        _patientRepository ??= new Repository<Patient>(_context);
    
    public async Task<int> SaveChangesAsync() =>
        await _context.SaveChangesAsync();
    
    public void Dispose() => _context?.Dispose();
}
```

### Usage in Service

```csharp
public class OrderService
{
    private readonly IUnitOfWork _unitOfWork;
    
    public OrderService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }
    
    public async Task<Order> CreateOrderAsync(CreateOrderDto dto)
    {
        // Create order
        var order = new Order { UserId = dto.UserId, Amount = dto.Amount };
        await _unitOfWork.Orders.AddAsync(order);
        
        // Update user stats
        var user = await _unitOfWork.Users.GetByIdAsync(dto.UserId);
        user.TotalOrders++;
        user.TotalSpent += order.Amount;
        await _unitOfWork.Users.UpdateAsync(user);
        
        // Save everything together
        await _unitOfWork.SaveChangesAsync();
        
        return order;
    }
}
```

### Dependency Injection

```csharp
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IOrderService, OrderService>();
```

---

## Specification Pattern

**Goal:** Encapsulate complex queries, reusable query logic.

```csharp
public abstract class Specification<T>
{
    public Expression<Func<T, bool>> Criteria { get; protected set; }
    public List<Expression<Func<T, object>>> Includes { get; } = new();
    public Expression<Func<T, object>> OrderBy { get; protected set; }
    public int Take { get; protected set; }
    public int Skip { get; protected set; }
    public bool IsPagingEnabled { get; protected set; }
}

// Specific specification
public class ActiveUsersSpecification : Specification<User>
{
    public ActiveUsersSpecification()
    {
        Criteria = u => u.Status == "Active";
        Includes.Add(u => u.Orders);
        OrderBy = u => u.CreatedAt;
    }
}

public class UsersByRoleSpecification : Specification<User>
{
    public UsersByRoleSpecification(string role, int pageNumber, int pageSize)
    {
        Criteria = u => u.Role == role;
        Skip = (pageNumber - 1) * pageSize;
        Take = pageSize;
        IsPagingEnabled = true;
    }
}
```

### Repository Using Specifications

```csharp
public class SpecificationRepository<T> : IRepository<T>
{
    private readonly DbContext _context;
    
    public async Task<IEnumerable<T>> GetBySpecificationAsync(Specification<T> spec)
    {
        var query = _context.Set<T>().AsQueryable();
        
        // Apply criteria
        if (spec.Criteria != null)
            query = query.Where(spec.Criteria);
        
        // Apply includes
        query = spec.Includes.Aggregate(query, (q, include) => q.Include(include));
        
        // Apply ordering
        if (spec.OrderBy != null)
            query = query.OrderBy(spec.OrderBy);
        
        // Apply paging
        if (spec.IsPagingEnabled)
            query = query.Skip(spec.Skip).Take(spec.Take);
        
        return await query.ToListAsync();
    }
}
```

### Usage

```csharp
var activeUsers = await _repository.GetBySpecificationAsync(
    new ActiveUsersSpecification());

var doctorsPage2 = await _repository.GetBySpecificationAsync(
    new UsersByRoleSpecification("Doctor", pageNumber: 2, pageSize: 10));
```

---

## CQRS Pattern (Command Query Responsibility Segregation)

**Goal:** Separate read and write models, optimize each independently.

```csharp
// COMMAND - Write operation
public class CreateUserCommand : ICommand
{
    public string Email { get; set; }
    public string Name { get; set; }
}

public class CreateUserCommandHandler : ICommandHandler<CreateUserCommand>
{
    private readonly DbContext _context;
    
    public async Task HandleAsync(CreateUserCommand command)
    {
        var user = new User { Email = command.Email, Name = command.Name };
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();
    }
}

// QUERY - Read operation
public class GetUserQuery : IQuery<UserDto>
{
    public int UserId { get; set; }
}

public class GetUserQueryHandler : IQueryHandler<GetUserQuery, UserDto>
{
    private readonly DbContext _context;
    
    public async Task<UserDto> HandleAsync(GetUserQuery query)
    {
        return await _context.Users
            .AsNoTracking()
            .Where(u => u.Id == query.UserId)
            .Select(u => new UserDto
            {
                Id = u.Id,
                Email = u.Email,
                Name = u.Name,
                OrderCount = u.Orders.Count
            })
            .FirstAsync();
    }
}
```

### Benefits

- ✅ Read queries optimized (projections, no tracking)
- ✅ Write commands optimized (relationships, validations)
- ✅ Can use different databases (read replica)
- ✅ Easy to test and reason about

---

## DbContext per Request (ASP.NET Core)

```csharp
// Program.cs
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Middleware ensures disposal
public class DbContextDisposalMiddleware
{
    private readonly RequestDelegate _next;
    
    public DbContextDisposalMiddleware(RequestDelegate next)
    {
        _next = next;
    }
    
    public async Task InvokeAsync(HttpContext context, IUnitOfWork unitOfWork)
    {
        try
        {
            await _next(context);
        }
        finally
        {
            unitOfWork.Dispose();
        }
    }
}
```

---

## Interview Q&A

**Q: When to use Repository pattern?**

A: When you want to abstract DbContext, make testing easier, and centralize data access logic.

**Q: Difference between Repository and Unit of Work?**

A:
- Repository: Single entity data access
- Unit of Work: Coordinates multiple repositories, single SaveChanges

**Q: How to test with Repository pattern?**

A:
```csharp
var mockRepository = new Mock<IRepository<User>>();
mockRepository.Setup(r => r.GetByIdAsync(1))
    .ReturnsAsync(new User { Id = 1, Email = "test@example.com" });

var service = new UserService(mockRepository.Object);
var user = await service.GetUserAsync(1);
Assert.Equal("test@example.com", user.Email);
```

**Q: When to use CQRS?**

A: When read and write patterns are very different, or when you have read replicas/event sourcing.

**Q: Specification pattern benefits?**

A: Encapsulates complex queries, reusable, testable, cleaner than scattered query logic.
