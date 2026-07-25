# Delegate, Event, Action, Func, Predicate

## Delegate (Foundation)

**Definition:** A type-safe function pointer. Blueprint for methods.

```csharp
// Define delegate type
public delegate void NotifyDelegate(string message);

// Method matching delegate signature
public void SendEmail(string message)
{
    Console.WriteLine($"Email: {message}");
}

// Usage
NotifyDelegate notify = SendEmail;
notify("Hello"); // Invokes SendEmail
```

---

## Action<T> (No Return)

**Definition:** Pre-defined delegate for methods that return `void`.

```csharp
// Instead of creating custom delegate
Action<string> sendEmail = (message) => Console.WriteLine($"Email: {message}");
sendEmail("Hello");

// Multiple actions
Action<int, int> add = (a, b) => Console.WriteLine($"Sum: {a + b}");
add(5, 3); // Output: Sum: 8
```

---

## Func<T, TResult> (With Return)

**Definition:** Pre-defined delegate for methods that return a value.

```csharp
// Syntax: Func<InputType1, InputType2, ..., ReturnType>
Func<int, int, int> multiply = (a, b) => a * b;
int result = multiply(5, 3); // 15

Func<string, int> stringLength = (s) => s.Length;
int len = stringLength("Ahmed"); // 5
```

---

## Predicate<T> (Boolean Return)

**Definition:** Special Func that always returns `bool`.

```csharp
// Predicate<T> = Func<T, bool>
Predicate<int> isEven = (n) => n % 2 == 0;
bool result = isEven(4); // true

Predicate<string> hasContent = (s) => !string.IsNullOrEmpty(s);
bool check = hasContent("Hello"); // true

// Real-world: Filtering
List<int> numbers = new { 1, 2, 3, 4, 5 };
var evens = numbers.FindAll(isEven); // [2, 4]
```

---

## Event (Publisher-Subscriber Pattern)

**Definition:** Type-safe way to implement observer pattern.

```csharp
// Step 1: Define delegate type
public delegate void OrderStatusChangedDelegate(string status);

// Step 2: Define event based on delegate
public class Order
{
    private OrderStatusChangedDelegate _statusChanged;
    public event OrderStatusChangedDelegate StatusChanged
    {
        add { _statusChanged += value; }
        remove { _statusChanged -= value; }
    }
    
    public void SetStatus(string status)
    {
        Console.WriteLine($"Status changed to: {status}");
        _statusChanged?.Invoke(status); // Notify subscribers
    }
}

// Step 3: Subscribe to event
var order = new Order();
order.StatusChanged += (status) => Console.WriteLine($"Email sent: Order is {status}");
order.StatusChanged += (status) => Console.WriteLine($"SMS sent: Order is {status}");

// Step 4: Trigger event
order.SetStatus("Shipped"); // Both subscribers notified
```

**Output:**
```
Status changed to: Shipped
Email sent: Order is Shipped
SMS sent: Order is Shipped
```

---

## Modern C# with EventHandler

```csharp
public class Order
{
    public event EventHandler<OrderStatusChangedEventArgs> StatusChanged;
    
    public void SetStatus(string status)
    {
        StatusChanged?.Invoke(this, new OrderStatusChangedEventArgs { Status = status });
    }
}

public class OrderStatusChangedEventArgs : EventArgs
{
    public string Status { get; set; }
}

// Usage
var order = new Order();
order.StatusChanged += (sender, e) => Console.WriteLine($"Order status: {e.Status}");
order.SetStatus("Shipped");
```

---

## Comparison Table

| Feature | Delegate | Action | Func | Predicate | Event |
|---------|----------|--------|------|-----------|-------|
| Purpose | Custom delegate | Void method | Method with return | Boolean return | Publish-Subscribe |
| Return Type | Custom | void | Any | bool | N/A |
| Built-in | No | Yes | Yes | Yes | Yes |
| Use Case | Custom logic | Fire & forget | Transformation | Filtering | Notifications |

---

## Interview Q&A

**Q: What's the difference between Delegate and Event?**

A: 
- Delegate is a type, Event is an instance of that type with +=/-= restrictions
- Event prevents external code from clearing subscriptions (only += and -=)
- Without Event, someone could do `eventDelegate = null;` and clear all subscribers

```csharp
// ❌ Bad - Delegate exposed
public delegate void OrderChanged;
public OrderChanged _changed; // Can be cleared externally
_changed = null; // External code can clear all subscribers!

// ✅ Good - Event protected
public event EventHandler OrderChanged; // External code can only += or -=
_changed = null; // ERROR - compiler prevents this
```

**Q: When to use Func vs Action?**

A:
- Use **Action** when you need fire-and-forget (no return value)
- Use **Func** when you need a result
- Use **Predicate** specifically for filtering operations (returns bool)
