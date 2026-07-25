# SOLID Principles

## S - Single Responsibility

```csharp
// ❌ BAD - Multiple responsibilities
public class User
{
    public string Email { get; set; }
    
    public void SaveToDatabase() { } // Database responsibility
    public void SendEmail() { } // Email responsibility
    public void GenerateReport() { } // Reporting responsibility
}

// ✅ GOOD - Single responsibility each
public class User
{
    public string Email { get; set; }
}

public class UserRepository
{
    public void Save(User user) { }
}

public class EmailService
{
    public void Send(User user) { }
}

public class ReportGenerator
{
    public void Generate(User user) { }
}
```

---

## O - Open/Closed Principle

```csharp
// ❌ BAD - Modify existing class for new payment methods
public class PaymentProcessor
{
    public void Process(string method, decimal amount)
    {
        if (method == "CreditCard")
            ProcessCreditCard(amount);
        else if (method == "PayPal")
            ProcessPayPal(amount);
        else if (method == "Crypto") // Add new method = modify class
            ProcessCrypto(amount);
    }
}

// ✅ GOOD - Extension without modification
public interface IPaymentMethod
{
    void Process(decimal amount);
}

public class CreditCardPayment : IPaymentMethod
{
    public void Process(decimal amount) { }
}

public class PayPalPayment : IPaymentMethod
{
    public void Process(decimal amount) { }
}

public class CryptoPayment : IPaymentMethod // Add new = no modification
{
    public void Process(decimal amount) { }
}

public class PaymentProcessor
{
    public void Process(IPaymentMethod method, decimal amount)
    {
        method.Process(amount); // Works with any implementation
    }
}
```

---

## L - Liskov Substitution Principle

```csharp
// ❌ BAD - Derived class breaks base contract
public class Bird
{
    public virtual void Fly() { }
}

public class Penguin : Bird
{
    public override void Fly()
    {
        throw new NotSupportedException("Penguins can't fly");
    }
}

// Usage breaks
Bird bird = new Penguin();
bird.Fly(); // Crashes!

// ✅ GOOD - Respects substitution
public abstract class Bird { }

public class FlyingBird : Bird
{
    public virtual void Fly() { }
}

public class Penguin : Bird
{
    public void Swim() { }
}

// Now works correctly
```

---

## I - Interface Segregation

```csharp
// ❌ BAD - Fat interface
public interface IWorker
{
    void Work();
    void Eat();
    void Sleep();
}

public class Robot : IWorker
{
    public void Work() { } // Can work
    public void Eat() { } // ERROR - Robots don't eat!
    public void Sleep() { } // ERROR - Robots don't sleep!
}

// ✅ GOOD - Segregated interfaces
public interface IWorkable
{
    void Work();
}

public interface IEatable
{
    void Eat();
}

public interface ISleepable
{
    void Sleep();
}

public class Robot : IWorkable
{
    public void Work() { } // Only implements what it needs
}

public class Human : IWorkable, IEatable, ISleepable
{
    public void Work() { }
    public void Eat() { }
    public void Sleep() { }
}
```

---

## D - Dependency Inversion

```csharp
// ❌ BAD - High-level depends on low-level
public class OrderService
{
    private readonly SqlDatabase _database = new();
    private readonly EmailSender _email = new();
    
    public void CreateOrder(Order order)
    {
        _database.Save(order); // Tightly coupled
        _email.Send(order); // Hard to test
    }
}

// ✅ GOOD - Both depend on abstraction
public interface IDatabase
{
    void Save(Order order);
}

public interface IEmailSender
{
    void Send(Order order);
}

public class OrderService
{
    private readonly IDatabase _database;
    private readonly IEmailSender _email;
    
    public OrderService(IDatabase database, IEmailSender email)
    {
        _database = database;
        _email = email;
    }
    
    public void CreateOrder(Order order)
    {
        _database.Save(order); // Can inject any implementation
        _email.Send(order); // Easy to mock for testing
    }
}

// Testing
var mockDb = new Mock<IDatabase>();
var mockEmail = new Mock<IEmailSender>();
var service = new OrderService(mockDb.Object, mockEmail.Object);
```

---

## Interview Q&A

**Q: Why SOLID matters?**

A:
- Maintainability: Easy to understand code
- Testability: Can mock dependencies
- Extensibility: Add features without modifying
- Reusability: Components work independently

**Q: Most important SOLID principle?**

A: Dependency Inversion - enables all others. If you only remember one, pick this.
