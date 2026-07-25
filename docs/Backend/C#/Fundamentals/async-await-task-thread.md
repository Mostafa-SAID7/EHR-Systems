# Async/Await, ConfigureAwait, Task vs Thread

## Async/Await Pattern

**Definition:** Non-blocking way to handle long-running operations.

```csharp
// ❌ Blocking (Synchronous)
public string FetchUser()
{
    Thread.Sleep(2000); // Blocks thread for 2 seconds
    return "User data";
}

// ✅ Non-blocking (Asynchronous)
public async Task<string> FetchUserAsync()
{
    await Task.Delay(2000); // Releases thread, resumes later
    return "User data";
}

// Usage
var result = await FetchUserAsync(); // Doesn't block calling thread
Console.WriteLine(result);
```

---

## How Async/Await Works

```csharp
public async Task<int> GetUserIdAsync()
{
    // await suspends here, returns control to caller
    int userId = await FetchUserIdFromDatabaseAsync();
    
    // Resumes after database call completes
    int score = CalculateScore(userId);
    
    return score;
}

// Behind the scenes - Compiler converts to State Machine
// Internally complex, but we code it simply
```

---

## Task vs Thread

| Feature | Thread | Task |
|---------|--------|------|
| What | OS-level construct | .NET abstraction |
| Overhead | High (~1MB per thread) | Low (~80 bytes) |
| Switching Cost | Expensive | Cheap |
| ThreadPool | Manual | Automatic |
| Async Support | No | Yes |
| Cancellation | Manual | Built-in |

```csharp
// ❌ Old way - Creating threads (wasteful)
var thread = new Thread(() =>
{
    DoWork();
});
thread.Start();
thread.Join(); // Wait for completion

// ✅ Modern way - Using Task (efficient)
var task = Task.Run(() => DoWork());
await task; // Wait for completion
```

---

## Task-based Async Pattern

```csharp
public async Task ProcessOrderAsync(int orderId)
{
    // Task returns no value
    var order = await FetchOrderAsync(orderId);
    await SaveOrderAsync(order);
}

public async Task<Order> FetchOrderAsync(int orderId)
{
    // Task<T> returns a value
    return await _database.Orders.FirstOrDefaultAsync(o => o.Id == orderId);
}

// Multiple operations in parallel
public async Task ProcessMultipleOrdersAsync(int[] orderIds)
{
    var tasks = orderIds.Select(id => ProcessOrderAsync(id)).ToList();
    await Task.WhenAll(tasks); // Wait for all to complete
}
```

---

## ConfigureAwait(false)

**Definition:** Prevents context capture, improves performance for library code.

```csharp
// ❌ Without ConfigureAwait
public async Task<string> GetUserAsync(int id)
{
    var user = await _db.Users.FindAsync(id); // Captures SynchronizationContext
    return user.Name;
}

// ✅ With ConfigureAwait(false)
public async Task<string> GetUserAsync(int id)
{
    var user = await _db.Users.FindAsync(id).ConfigureAwait(false);
    return user.Name;
}
```

**Why use ConfigureAwait(false)?**

1. **Performance**: Avoids context switching overhead
2. **Prevents Deadlocks**: In UI apps, not capturing context prevents sync-over-async issues
3. **Library Code**: Best practice for library/service code

```csharp
// Dangerous - can deadlock
public string GetUserSync()
{
    return GetUserAsync(1).Result; // Blocks, waiting for async
}

// If GetUserAsync() tries to capture UI context, deadlock!
// ConfigureAwait(false) prevents this
```

---

## Interview Q&A

**Q: What's the difference between Task.Run and Task.Factory.StartNew?**

A:
```csharp
// Task.Run - Simpler, preferred in modern code
Task.Run(() => DoWork());

// Task.Factory.StartNew - More control, legacy
Task.Factory.StartNew(() => DoWork(), TaskScheduler.Default);
```

Task.Run is usually what you want because:
- Uses ThreadPool efficiently
- Simpler syntax
- Better for async/await

**Q: Should I use Task.Result or await?**

A:
```csharp
// ❌ Never do this
var result = GetUserAsync().Result; // Can deadlock!

// ✅ Always use await
var result = await GetUserAsync();

// If you MUST block (rare):
var result = GetUserAsync().GetAwaiter().GetResult();
```

**Q: When use ConfigureAwait(false)?**

A:
- ✅ Always in library/service code
- ❌ Rarely in UI code (ASP.NET Core is fine)
- Reason: Prevents deadlocks, improves performance

**Q: Task.Delay vs Thread.Sleep?**

A:
```csharp
// Thread.Sleep - Blocks thread (DON'T USE in async)
Thread.Sleep(1000); // Thread stuck for 1 second

// Task.Delay - Non-blocking (USE in async)
await Task.Delay(1000); // Thread free, resumes later
```
