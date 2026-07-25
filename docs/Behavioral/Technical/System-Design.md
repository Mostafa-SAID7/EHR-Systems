# System Design: Medical Coding Automation Platform

Architectural blueprint for an enterprise-grade AI medical coding platform.

---

## 🏛️ High-Level Architecture

```
[EHR / Clinic System] ── (HL7 / FHIR API) ──> [API Gateway]
                                                   │
                        ┌──────────────────────────┴──────────────────────────┐
                        ▼                                                     ▼
              [Coding Orchestrator]                                   [Audit & Compliance]
                        │                                                     │
        ┌───────────────┴───────────────┐                                     │
        ▼                               ▼                                     │
[AI Inference Service]        [Rule Engine Cache]                                 │
(Python ML Container)          (Redis / PostgreSQL)                                 │
        │                               │                                     │
        └───────────────┬───────────────┘                                     │
                        ▼                                                     ▼
            [Kafka Event Stream] ─────────────────────────────────> [PostgreSQL DB]
```

---

## ⚡ Key Technical Tradeoffs

1. **Synchronous vs Asynchronous Processing**:
   - **Real-Time UI Suggestion**: REST call directly to Coding Orchestrator (< 400ms target).
   - **Batch Discharge Processing**: Kafka event stream consuming hospital visit feeds asynchronously.

2. **Caching Strategy**:
   - Common diagnosis patterns (e.g., Routine Diabetes Follow-up → ICD-10 E11.9) cached in Redis to eliminate AI model execution costs.

3. **Audit Trail Guarantee**:
   - Outbox pattern used to ensure every AI suggestion override by a medical coder emits an immutable audit event to PostgreSQL.
