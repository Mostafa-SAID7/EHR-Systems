# ASP.NET Core Middleware Pipeline

## Request Flow Diagram

```
┌─────────────────────────────────────────────────────────────┐
│                        REQUEST ARRIVES                       │
└──────────────────────────┬──────────────────────────────────┘
                           ↓
        ┌──────────────────────────────────────┐
        │  Middleware 1 (CORS, Security)       │
        │  ↓                                    │↑
        └────────────┬─────────────────────────┘
                     ↓
        ┌──────────────────────────────────────┐
        │  Middleware 2 (Authentication)       │
        │  ↓                                    │↑
        └────────────┬─────────────────────────┘
                     ↓
        ┌──────────────────────────────────────┐
        │  Middleware 3 (Authorization)        │
        │  ↓                                    │↑
        └────────────┬─────────────────────────┘
                     ↓
        ┌──────────────────────────────────────┐
        │  Middleware 4 (Routing)              │
        │  ↓                                    │↑
        └────────────┬─────────────────────────┘
                     ↓
        ┌──────────────────────────────────────┐
        │  Middleware 5 (EndpointMiddleware)   │
        │  ↓                                    │↑
        └────────────┬─────────────────────────┘
                     ↓
        ┌──────────────────────────────────────┐
        │  Controller Action                   │
        │  ↓                                    │↑
        └────────────┬─────────────────────────┘
                     ↓
        ┌──────────────────────────────────────┐
        │  Service Layer                       │
        │  ↓                                    │↑
        └────────────┬─────────────────────────┘
                     ↓
        ┌──────────────────────────────────────┐
        │  Repository Layer                    │
        │  ↓                                    │↑
        └────────────┬─────────────────────────┘
                     ↓
        ┌──────────────────────────────────────┐
        │  Database Query                      │
        │  ↓                                    │↑
        └────────────┬─────────────────────────┘
                     ↓
        ┌──────────────────────────────────────┐
        │  Generate Response                   │
        │  ↑                                    │↓
        └─────────────┬────────────────────────┘
                      ↓
       Response flows back through middleware
       (reverse order) → Client
```

---

## Middleware Configuration (Program.cs)

```csharp
var builder = WebApplicationBuilder.CreateBuilder(args);

// Add services to DI container
builder.Services.AddControllers();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

var app = builder.Build();

// Configure middleware pipeline (ORDER MATTERS!)
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage(); // Error handling
}

app.UseHttpsRedirection(); // HTTPS redirection
app.UseCors("AllowAll"); // CORS
app.UseAuthentication(); // Identify user (get claims)
app.UseAuthorization(); // Check permissions (claims)
app.UseRouting(); // Route to controller
app.MapControllers(); // Execute endpoint

app.Run();
```

---

## Custom Middleware Example

```csharp
// 1. Create middleware class
public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;
    
    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }
    
    public async Task InvokeAsync(HttpContext context)
    {
        // PRE-PROCESSING (Request)
        _logger.LogInformation($"Request: {context.Request.Method} {context.Request.Path}");
        
        // Pass to next middleware
        await _next(context);
        
        // POST-PROCESSING (Response)
        _logger.LogInformation($"Response: {context.Response.StatusCode}");
    }
}

// 2. Register in Program.cs
app.UseMiddleware<RequestLoggingMiddleware>();
```

---

## Understanding DI Lifetimes

### Scoped (Per Request)

```csharp
public interface IUserService { }
public class UserService : IUserService { }

builder.Services.AddScoped<IUserService, UserService>();

// SAME instance for entire HTTP request
// Different instance for next request
// Perfect for: DbContext, UnitOfWork
```

```csharp
[ApiController]
public class UserController : ControllerBase
{
    private readonly IUserService _service;
    
    public UserController(IUserService service)
    {
        _service = service; // Same instance throughout request
    }
    
    [HttpGet]
    public async Task<IActionResult> GetUser()
    {
        var user = await _service.GetUserAsync();
        return Ok(user); // _service reused
    }
}
```

### Singleton (Application Lifetime)

```csharp
builder.Services.AddSingleton<ICacheService, CacheService>();

// SAME instance throughout application lifetime
// Created once, reused forever
// Perfect for: Cache, Settings, Single-instance services
```

### Transient (Every Request)

```csharp
builder.Services.AddTransient<IRepository, Repository>();

// NEW instance every time it's requested
// Created multiple times
// Avoid: Stateful services
```

---

## Dependency Injection Why?

### Without DI (❌ Tightly Coupled)

```csharp
public class UserController
{
    private readonly UserService _service;
    
    public UserController()
    {
        _service = new UserService(); // Hard dependency
        // Can't test without real UserService
        // Can't swap implementations
    }
}
```

### With DI (✅ Loosely Coupled)

```csharp
public interface IUserService
{
    Task<User> GetUserAsync(int id);
}

public class UserController
{
    private readonly IUserService _service;
    
    public UserController(IUserService service) // Injected
    {
        _service = service; // Can be mock in tests
    }
}

// Testing
var mockService = new Mock<IUserService>();
var controller = new UserController(mockService.Object); // Inject mock
```

---

## Why Middleware Order Matters?

```csharp
// ❌ WRONG ORDER
app.UseAuthorization(); // Check permissions first
app.UseAuthentication(); // Identify user second (user not identified yet!)
app.MapControllers();

// ✅ CORRECT ORDER
app.UseAuthentication(); // Identify user first
app.UseAuthorization(); // Check permissions second (now we know user)
app.MapControllers();
```

---

## Interview Q&A

**Q: Why Middleware?**

A: Middleware provides cross-cutting concerns:
- Logging
- Authentication/Authorization
- Error handling
- CORS
- Request/Response transformation
- Security headers

**Q: Why DI?**

A:
- Loose coupling (swap implementations)
- Testability (inject mocks)
- Reusability
- Lifecycle management

**Q: Scoped vs Singleton for DbContext?**

A:
```csharp
// ✅ CORRECT - Scoped
builder.Services.AddScoped<DbContext>();
// Fresh instance per request, disposed after request
// Prevents: Stale data, connection leaks, concurrency issues

// ❌ WRONG - Singleton
builder.Services.AddSingleton<DbContext>();
// Same instance forever - thread-safety issues, memory leaks
```

**Q: What happens if middleware is in wrong order?**

A: 
- Authorization before Authentication = user not identified
- Routing after controller execution = routing never happens
- Exception middleware too late = exceptions not caught
