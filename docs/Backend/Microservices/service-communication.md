# Service-to-Service Communication

## REST API (Synchronous)

```csharp
// Order Service calls Payment Service
public class OrderService
{
    private readonly HttpClient _httpClient;
    
    public async Task<Order> CreateOrderAsync(Order order)
    {
        // Call Payment Service synchronously
        var paymentResponse = await _httpClient.PostAsJsonAsync(
            "http://payment-service/api/payments",
            new { amount = order.Total, orderId = order.Id }
        );
        
        if (!paymentResponse.IsSuccessStatusCode)
            throw new PaymentFailedException("Payment processing failed");
        
        // Proceed only after payment succeeds
        return await _repository.SaveAsync(order);
    }
}
```

**Pros:** Simple, immediate feedback  
**Cons:** Tight coupling, cascading failures

---

## Message Queue (Asynchronous)

```csharp
// Order Service publishes event
public class OrderService
{
    private readonly IMessagePublisher _publisher;
    
    public async Task<Order> CreateOrderAsync(Order order)
    {
        // Save order immediately
        var saved = await _repository.SaveAsync(order);
        
        // Publish event (fire and forget)
        await _publisher.PublishAsync("order.created", new
        {
            OrderId = saved.Id,
            Amount = saved.Total
        });
        
        return saved;
    }
}

// Payment Service listens to event
public class PaymentService
{
    private readonly IMessageSubscriber _subscriber;
    
    public void Subscribe()
    {
        _subscriber.Subscribe("order.created", async message =>
        {
            var payment = await ProcessPaymentAsync(message);
            
            // Publish result event
            await _publisher.PublishAsync("payment.completed", new
            {
                OrderId = message.OrderId,
                Success = payment.Success
            });
        });
    }
}
```

**Pros:** Loose coupling, resilience, scalability  
**Cons:** Eventual consistency, complexity

---

## Interview Q&A

**Q: When to use REST vs Message Queue?**

A:
- REST: Immediate response needed, strong consistency
- Message Queue: Can wait, eventual consistency acceptable, high availability needed
