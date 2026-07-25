# System Design — Scalability, Resilience & Deployment

## 1. Horizontal vs Vertical Scaling

| Aspect | Vertical (Scale Up) | Horizontal (Scale Out) |
|:--- |:--- |:--- |
| **Method** | Bigger server (more RAM/CPU) | More server instances behind a load balancer |
| **Cost** | Expensive — hardware limits | Cheaper — commodity hardware |
| **Failure risk** | Single point of failure | Resilient — one node fails, others continue |
| **Use case** | DB primary nodes, legacy apps | Stateless APIs, microservices |

**Rule**: Prefer stateless services behind a load balancer. Session/state must live in Redis, not in-process memory.

---

## 2. Load Balancing Strategies

- **Round Robin**: Distributes equally; default for homogeneous services.
- **Least Connections**: Routes to instance with fewest active connections; better for variable-duration requests.
- **Consistent Hashing**: Routes same user/tenant to same node; useful for cache affinity.

---

## 3. CAP Theorem

> In a distributed system, you can guarantee **at most 2** of: **Consistency**, **Availability**, **Partition Tolerance**.

| System Type | Prioritizes | Example |
|:--- |:--- |:--- |
| **CP** | Consistency + Partition | PostgreSQL, MongoDB (strong consistency mode) |
| **AP** | Availability + Partition | Cassandra, DynamoDB (eventual consistency) |

**EHR / Healthcare Rule**: Choose CP. Stale medical data is more dangerous than brief unavailability.

---

## 4. Resilience Patterns

### Circuit Breaker (Polly)
```csharp
var circuitBreaker = Policy
    .Handle<HttpRequestException>()
    .CircuitBreakerAsync(
        exceptionsAllowedBeforeBreaking: 3,
        durationOfBreak: TimeSpan.FromSeconds(30));
```

### Retry with Exponential Backoff (Polly)
```csharp
var retry = Policy
    .Handle<HttpRequestException>()
    .WaitAndRetryAsync(3, attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)));
```

### Timeout
```csharp
var timeout = Policy.TimeoutAsync<HttpResponseMessage>(TimeSpan.FromSeconds(5));
```

### Bulkhead (Limit Concurrent Calls)
```csharp
var bulkhead = Policy.BulkheadAsync(maxParallelization: 10, maxQueuingActions: 25);
```

---

## 5. API Gateway Pattern

Single entry point for all clients:
- Rate limiting & throttling
- Authentication (JWT validation before routing)
- Request routing to downstream microservices
- SSL termination
- Response caching

```
Client → [API Gateway] → [Coding Service]
                       → [Audit Service]
                       → [Claim Service]
```

---

## 6. Event-Driven Architecture (Kafka)

```
[Visit Created Event]
         │
    [Kafka Topic: visit-events]
         │
    ┌────┴────────────┐
    ▼                 ▼
[Coding Consumer]  [Audit Consumer]
(Suggest codes)    (Record audit log)
```

**Key rules**:
- Events are immutable facts (past tense: `VisitCreated`, `ClaimSubmitted`).
- Consumers are idempotent (safe to replay on failure).
- Use Outbox pattern to guarantee event delivery with DB writes.

---

## 7. Deployment: Docker + Kubernetes

```yaml
# Kubernetes Deployment
apiVersion: apps/v1
kind: Deployment
metadata:
  name: coding-service
spec:
  replicas: 3
  selector:
    matchLabels:
      app: coding-service
  template:
    spec:
      containers:
      - name: coding-service
        image: tachyhealth/coding-service:latest
        resources:
          requests: { cpu: "250m", memory: "256Mi" }
          limits:   { cpu: "500m", memory: "512Mi" }
        livenessProbe:
          httpGet: { path: /health/live, port: 8080 }
        readinessProbe:
          httpGet: { path: /health/ready, port: 8080 }
```
