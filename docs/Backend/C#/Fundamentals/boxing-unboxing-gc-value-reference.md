# Boxing, Unboxing, Garbage Collector, Value Type, Reference Type

## Value Type vs Reference Type

| Feature | Value Type | Reference Type |
|---------|-----------|-----------------|
| Where Stored | Stack | Heap |
| Assignment | Copies value | Copies reference |
| Speed | Faster | Slower |
| Lifetime | Local scope | GC managed |
| Examples | int, double, struct | class, string, delegate |
| Null | Cannot be null | Can be null |

```csharp
// VALUE TYPE (Stack)
int a = 5;
int b = a; // Copies value
b = 10;
Console.WriteLine(a); // Still 5

// REFERENCE TYPE (Heap)
var user1 = new User { Name = "Ahmed" };
var user2 = user1; // Copies reference
user2.Name = "Ali";
Console.WriteLine(user1.Name); // Now "Ali" (same object)
```

---

## Boxing (Value → Reference)

**Definition:** Wrapping value type in object reference.

```csharp
int number = 5; // Value type on stack

// BOXING - Wrap in object
object boxed = number; // Copies to heap in object wrapper

// Behind the scenes:
// 1. Allocates heap memory
// 2. Copies value into object
// 3. Returns reference to object

Console.WriteLine(boxed); // "5"
```

**When Boxing Occurs:**
```csharp
// Explicit boxing
object box1 = 5;
object box2 = true;

// Implicit boxing
ArrayList list = new ArrayList();
list.Add(5); // Automatically boxes int
list.Add("text");

// Boxing in generic collections (old code)
Hashtable hash = new Hashtable();
hash.Add("age", 30); // Boxes int

// Boxing in method calls
void PrintValue(object obj) { }
PrintValue(5); // Boxes int automatically
```

---

## Unboxing (Reference → Value)

**Definition:** Extracting value from boxed object.

```csharp
object boxed = 5; // Boxed int

// UNBOXING - Extract value
int unboxed = (int)boxed; // Must match original type!

// ❌ DANGEROUS - Type mismatch
object box = 5; // Boxed as int
double value = (double)box; // ERROR! Must unbox as int first

// ✅ CORRECT
object box = 5;
int value = (int)box; // Correct type
double converted = (double)value; // Now can convert
```

---

## Boxing Performance Impact

```csharp
// ❌ BAD - Lots of boxing
ArrayList list = new ArrayList();
for (int i = 0; i < 1_000_000; i++)
{
    list.Add(i); // Boxes each int!
}
// Result: 1 million allocations, 1 million GC pressure

// ✅ GOOD - No boxing with generics
List<int> list = new List<int>();
for (int i = 0; i < 1_000_000; i++)
{
    list.Add(i); // No boxing
}
// Result: Single heap allocation for entire list
```

**Boxing/Unboxing Cost:**
- Boxing: ~100x slower than direct assignment
- Unboxing: Similar overhead
- More importantly: GC pressure from allocations

---

## Garbage Collector (GC)

**Definition:** Automatic memory management system in .NET.

### How GC Works

```
Gen 0 (Young objects)
    ↓
Gen 1 (Medium-lived)
    ↓
Gen 2 (Long-lived)
    ↓
Large Object Heap (>85KB)
```

```csharp
// GC automatically reclaims unreferenced objects
var user = new User { Name = "Ahmed" };
DoSomething(user);
// After function returns, user goes out of scope
// GC will eventually reclaim the memory
```

---

## GC Generations

```csharp
// Generation 0 - Collected frequently
var temp1 = new User();
var temp2 = new User();
// Short-lived objects

// Generation 1 - Intermediate lifetime
var cached = new User(); // Survives Gen 0 collection

// Generation 2 - Long-lived
static User singleton = new User(); // Never collected
```

---

## Memory Leaks in C#

```csharp
// ❌ Memory leak - Event not unsubscribed
public class UserService
{
    private static event EventHandler OnUserCreated;
    
    public void Subscribe(EventHandler handler)
    {
        OnUserCreated += handler; // Never unsubscribed
    } // User stays in memory forever!
}

// ✅ Fix - Properly unsubscribe
public void Unsubscribe(EventHandler handler)
{
    OnUserCreated -= handler;
}

// ✅ Better - Use IDisposable
public class UserService : IDisposable
{
    private EventHandler _handler;
    
    public void Dispose()
    {
        OnUserCreated -= _handler; // Cleanup on dispose
    }
}
```

---

## IDisposable Pattern

```csharp
public class DatabaseConnection : IDisposable
{
    private bool _disposed = false;
    
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this); // Don't call finalizer
    }
    
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed) return;
        
        if (disposing)
        {
            // Dispose managed resources
            _connection?.Close();
        }
        
        _disposed = true;
    }
    
    ~DatabaseConnection() // Finalizer as safety net
    {
        Dispose(false);
    }
}

// Usage
using (var db = new DatabaseConnection())
{
    // Use connection
} // Dispose called automatically
```

---

## Modern C# 8+ - Using Declaration

```csharp
// Old way
using (var db = new DatabaseConnection())
{
    // Use
} // Disposed here

// Modern way (C# 8+)
using var db = new DatabaseConnection();
// Use
// Disposed automatically at end of scope
```

---

## Interview Q&A

**Q: What's the difference between struct and class?**

A:
```csharp
// Struct - Value type (stack)
public struct Point
{
    public int X { get; set; }
    public int Y { get; set; }
}

var p1 = new Point { X = 5, Y = 10 };
var p2 = p1; // Copies value
p2.X = 20;
Console.WriteLine(p1.X); // Still 5

// Class - Reference type (heap)
public class User
{
    public string Name { get; set; }
}

var u1 = new User { Name = "Ahmed" };
var u2 = u1; // Copies reference
u2.Name = "Ali";
Console.WriteLine(u1.Name); // Now "Ali"
```

**Q: When to use struct?**

A:
- Small data structures (< 16 bytes)
- Immutable values
- High-frequency allocations
- Examples: Point, Vector, DateTime

**Q: What happens when struct is assigned to object?**

A: Boxing occurs - value copied to heap in object wrapper. This is why large structs are bad - expensive boxing.

**Q: How to prevent memory leaks?**

A:
- Implement IDisposable for resources
- Unsubscribe from events
- Use `using` statement
- Avoid static event handlers
- Set reference to null in finalizers
