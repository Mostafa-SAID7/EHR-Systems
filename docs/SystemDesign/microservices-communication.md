# Microservices Architecture & Communication

## Microservices Pattern

```
┌─────────────────────────────────────────────────────────┐
│                    API Gateway                          │
└────────────┬────────────┬────────────┬──────────────────┘
             ↓            ↓            ↓
        ┌─────────┐  ┌─────────┐  ┌─────────┐
        │ User    │  │ Order   │  │ Payment │
        │ Service │  │ Service │  │ Service │
        └────┬────┘  └────┬────┘  └────┬────┘
             ↓            ↓            ↓
        ┌─────────┐  ┌─────────┐  ┌─────────┐
        │ User DB │  │ Order DB│  │ Payment │
        │         │  │         │  │   DB    │
        └─────────┘  └─────────┘  └─────────┘
```

---

## Synchronous Communication (REST)

```csharp
// Order Service calls Payment Service
public class OrderService
{
    private readonly HttpClient _httpClient;
    
    public async Task<Order> CreateOrderAsync(Order order)
    {
        // Sync call to Payment Service
        var payment = await _httpClient.PostAsync(
            "http://payment-service/process",
            new StringContent(JsonConvert.SerializeObject(order.PaymentInfo))
        );
        
        if (!payment.IsSuccessStatusCode)
            throw new Exception("Payment failed");
        
        // Save order only if payment succeeds
        return await _orderRepository.SaveAsync(order);
    }
}
```

**Pros:** Simple, immediate feedback  
**Cons:** Tight coupling, cascading failures

---

## Asynchronous Communication (Message Queue)

```csharp
// Order Service publishes event
public class OrderService
{
    private readonly IMessagePublisher _publisher;
    
    public async Task<Order> CreateOrderAsync(Order order)
    {
        // 1. Create order
        var savedOrder = await _orderRepository.SaveAsync(order);
        
        // 2. Publish event (fire and forget)
        await _publisher.PublishAsync("order.created", new OrderCreatedEvent
        {
            OrderId = savedOrder.Id,
            Amount = savedOrder.Total,
            UserId = savedOrder.UserId
        });
        
        return savedOrder; // Return immediately
    }
}

// Payment Service listens
public class PaymentService
{
    [MessageHandler("order.created")]
    public async Task HandleOrderCreated(OrderCreatedEvent @event)
    {
        // Process payment asynchronously
        var result = await ProcessPaymentAsync(@event.Amount);
        
        // Publish result event
        await _publisher.PublishAsync("payment.processed", new PaymentProcessedEvent
        {
            OrderId = @event.OrderId,
            Success = result.Success
        });
    }
}
```

**Pros:** Loose coupling, resilience  
**Cons:** Complex, eventual consistency

---

## Service Discovery

```csharp
// Without Service Discovery (Hard-coded)
var paymentServiceUrl = "http://payment-service:5000";

// With Service Discovery (Dynamic)
var paymentServiceUrl = await _serviceRegistry.GetServiceUrlAsync("payment-service");

// Load Balancing
var availableInstances = await _serviceRegistry.GetHealthyInstancesAsync("payment-service");
var selectedInstance = _loadBalancer.SelectRoundRobin(availableInstances);
```

---

## Resilience Patterns

### Circuit Breaker

```csharp
// Auto-fail when service is down
public class ResilientHttpClient
{
    private CircuitBreakerPolicy _policy = Policy
        .Handle<HttpRequestException>()
        .OrResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
        .CircuitBreakerAsync<HttpResponseMessage>(
            handledEventsAllowedBeforeBreaking: 3,
            durationOfBreak: TimeSpan.FromSeconds(30)
        );
    
    public async Task<T> GetAsync<T>(string url)
    {
        var response = await _policy.ExecuteAsync(async () =>
            await _httpClient.GetAsync(url)
        );
        
        return JsonConvert.DeserializeObject<T>(
            await response.Content.ReadAsStringAsync()
        );
    }
}
```

### Retry with Backoff

```csharp
var retryPolicy = Policy
    .Handle<HttpRequestException>()
    .WaitAndRetryAsync(
        retryCount: 3,
        sleepDurationProvider: attempt =>
            TimeSpan.FromSeconds(Math.Pow(2, attempt)) // 2s, 4s, 8s
    );
```

---

## Interview Q&A

**Q: Sync vs Async communication in microservices?**

A:
- Sync (REST): Tight coupling, immediate feedback, cascading failures
- Async (Events): Loose coupling, resilience, eventual consistency

**Q: How to prevent cascading failures?**

A:
- Circuit Breaker: Fail fast when service down
- Timeout: Don't wait indefinitely
- Retry: Handle transient failures
- Fallback: Return sensible default

**Q: Service-to-service authentication?**

A: Service mesh (Istio) or API keys with mTLS (mutual TLS)
