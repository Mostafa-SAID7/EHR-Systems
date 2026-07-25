# Repository Pattern & Unit of Work

## Problem Without Repository

```csharp
// ❌ Tightly coupled to EF
public class UserController
{
    private readonly DbContext _context;
    
    [HttpGet("{id}")]
    public async Task<User> GetUser(int id)
    {
        // Direct EF usage - hard to test
        return await _context.Users.FindAsync(id);
    }
    
    [HttpPost]
    public async Task<User> CreateUser(User user)
    {
        // Can't test without real database
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return user;
    }
}
```

---

## Solution: Repository Pattern

```csharp
// Step 1: Define interface (abstraction)
public interface IUserRepository
{
    Task<User> GetByIdAsync(int id);
    Task<List<User>> GetAllAsync();
    Task AddAsync(User user);
    Task UpdateAsync(User user);
    Task DeleteAsync(int id);
}

// Step 2: Implement repository
public class UserRepository : IUserRepository
{
    private readonly DbContext _context;
    
    public async Task<User> GetByIdAsync(int id)
    {
        return await _context.Users.FindAsync(id);
    }
    
    public async Task<List<User>> GetAllAsync()
    {
        return await _context.Users.ToListAsync();
    }
    
    public async Task AddAsync(User user)
    {
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
    }
    
    // ... other methods
}

// Step 3: Inject and use
public class UserController
{
    private readonly IUserRepository _repository;
    
    public UserController(IUserRepository repository)
    {
        _repository = repository;
    }
    
    [HttpGet("{id}")]
    public async Task<User> GetUser(int id)
    {
        return await _repository.GetByIdAsync(id);
    }
}

// Step 4: Test with mock
var mockRepo = new Mock<IUserRepository>();
mockRepo.Setup(r => r.GetByIdAsync(1))
    .ReturnsAsync(new User { Id = 1, Name = "Ahmed" });

var controller = new UserController(mockRepo.Object);
var user = await controller.GetUser(1);
```

---

## Unit of Work Pattern

```csharp
// Step 1: Define UoW interface
public interface IUnitOfWork
{
    IUserRepository Users { get; }
    IOrderRepository Orders { get; }
    Task<int> SaveChangesAsync();
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}

// Step 2: Implement
public class UnitOfWork : IUnitOfWork
{
    private readonly DbContext _context;
    
    public IUserRepository Users { get; private set; }
    public IOrderRepository Orders { get; private set; }
    
    public UnitOfWork(DbContext context)
    {
        _context = context;
        Users = new UserRepository(context);
        Orders = new OrderRepository(context);
    }
    
    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }
}

// Step 3: Use in service
public class OrderService
{
    private readonly IUnitOfWork _unitOfWork;
    
    public async Task<Order> CreateOrderAsync(Order order, User user)
    {
        using (var transaction = await _unitOfWork.BeginTransactionAsync())
        {
            try
            {
                // Update user balance
                user.Balance -= order.Total;
                await _unitOfWork.Users.UpdateAsync(user);
                
                // Create order
                await _unitOfWork.Orders.AddAsync(order);
                
                // Save all changes together
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();
                
                return order;
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }
    }
}
```

---

## Benefits

✅ Loose coupling - Easy to swap implementations  
✅ Testable - Inject mocks  
✅ Centralized - All data access in one place  
✅ Transactions - Unit of work handles multiple saves
