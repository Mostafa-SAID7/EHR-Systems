# Class vs Record in C#

## Quick Comparison

| Feature | Class | Record |
|---------|-------|--------|
| Type | Reference | Reference |
| Mutability | Mutable | Immutable |
| Equality | Reference | Value-based |
| ToString() | Manual | Auto |
| Equals() | Manual | Auto |
| GetHashCode() | Manual | Auto |
| Use Case | Business Logic | Data Transfer |

---

## Class (Reference Type - Mutable)

```csharp
public class Person
{
    public string Name { get; set; }
    public int Age { get; set; }
}

var p1 = new Person { Name = "Ahmed", Age = 30 };
var p2 = p1;
p2.Name = "Ali";
Console.WriteLine(p1.Name); // Ali (same reference)
Console.WriteLine(ReferenceEquals(p1, p2)); // true
```

---

## Record (Reference Type - Immutable)

```csharp
public record Person(string Name, int Age);

var p1 = new Person("Ahmed", 30);
var p2 = p1 with { Name = "Ali" }; // Creates new instance
Console.WriteLine(p1.Name); // Ahmed (unchanged)
Console.WriteLine(p1 == p2); // false (different values)
Console.WriteLine(new Person("Ahmed", 30) == new Person("Ahmed", 30)); // true (value equality)
```

---

## Why Records for DTOs?

```csharp
// ❌ Bad - Mutable DTO can cause bugs
public class UserDto
{
    public string Email { get; set; }
    public string Role { get; set; }
}

var user = new UserDto { Email = "user@example.com", Role = "User" };
SendToDatabase(user);
// Someone could have modified user during processing

// ✅ Good - Immutable DTO guarantees integrity
public record UserDto(string Email, string Role);

var user = new UserDto("user@example.com", "User");
SendToDatabase(user); // user is guaranteed unchanged
```

---

## Interview Q&A

**Q: When to use Record over Class?**

A: Use records when:
- Data is immutable by design (DTOs, API responses)
- Value equality semantics needed
- Want auto-generated `Equals()`, `GetHashCode()`, `ToString()`
- Data container pattern (not business logic)

**Q: What's immutability benefit?**

A: 
- Thread-safe without locks
- Can be used as dictionary keys safely
- Easier to reason about (data won't change)
- Great for caching and distributed systems
