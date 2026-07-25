# System Design — Architecture Patterns (CQRS, Event Sourcing, Saga, BFF)

## 1. Clean Architecture Layers

```
┌─────────────────────────────┐
│       Presentation          │  ← Controllers, API Endpoints
├─────────────────────────────┤
│       Application           │  ← Use Cases, MediatR Handlers, DTOs
├─────────────────────────────┤
│         Domain              │  ← Entities, Aggregates, Domain Events, Interfaces
├─────────────────────────────┤
│     Infrastructure          │  ← EF Core, Kafka, Redis, HTTP Clients
└─────────────────────────────┘
```
**Dependency Rule**: Inner rings never depend on outer rings.

---

## 2. Event Sourcing

Instead of storing current state, store every state-changing event:

```
VisitCreated    { visitId: 1, patientId: 10, timestamp: ... }
CodeSuggested   { visitId: 1, codes: ["E11.9"], source: "AI" }
CodeConfirmed   { visitId: 1, codes: ["E11.9"], coderId: 5  }
ClaimSubmitted  { visitId: 1, claimId: 99, timestamp: ...   }
```

- **Audit trail for free** — the event log IS the audit log (critical for HIPAA).
- **Replay capability** — rebuild any projection by replaying events.
- **Drawback** — eventual consistency; queries need projection stores.

---

## 3. Saga Pattern (Distributed Transactions)

**Choreography Saga** (events trigger next step):
```
[ClaimSubmitted Event]
        │
[InsuranceVerificationService] → VerifyAsync() → [InsuranceVerified Event]
        │
[PaymentService] → ProcessAsync()
```

**Orchestration Saga** (central coordinator):
```csharp
public class ClaimProcessingSaga
{
    public async Task OrchestateAsync(int claimId)
    {
        await _insurance.VerifyAsync(claimId);   // Step 1
        await _payment.ProcessAsync(claimId);    // Step 2
        await _audit.RecordCompletedAsync(claimId); // Step 3
        // On failure → compensating actions run in reverse
    }
}
```

---

## 4. Backend for Frontend (BFF)

Create a dedicated API layer per client type rather than one monolithic API:

```
Mobile App    → [Mobile BFF]  → Microservices
Web Dashboard → [Web BFF]     → Microservices
3rd Party     → [Public API]  → Microservices
```

**Benefits**: Each BFF shapes responses for its client; no over-fetching/under-fetching.
