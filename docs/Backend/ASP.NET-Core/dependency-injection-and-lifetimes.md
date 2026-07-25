# ASP.NET Core Dependency Injection & Security

Deep dive into DI service lifetimes (`Transient`, `Scoped`, `Singleton`), custom middleware, and JWT security configuration.

---

## 1. Service Lifetime Guidelines

- **`Transient`**: Short-lived, stateless services (e.g., lightweight calculators/mappers).
- **`Scoped`**: Created once per HTTP request (e.g., `DbContext`, Unit of Work repositories).
- **`Singleton`**: Single instance for the application lifetime (e.g., in-memory cache, background worker queues).

> ⚠️ **Captive Dependency Warning**: Never inject a `Scoped` service into a `Singleton` service! Use `IServiceScopeFactory` to resolve scoped services safely inside singletons.

```csharp
public class QueueProcessorSingleton : IHostedService
{
    private readonly IServiceScopeFactory _scopeFactory;

    public QueueProcessorSingleton(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public async Task ProcessItemAsync()
    {
        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<EhrDbContext>();
        // Process db operations safely
    }
}
```
