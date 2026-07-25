# Technical Interview Scenarios & Problem Solving

Real-world technical scenarios and architectural answers.

---

## Scenario 1: AI Model Latency Spikes During Peak Clinic Discharge Hours

- **Problem**: AI Inference container latency jumps from 200ms to 4s under load.
- **Solution**:
  1. Implement Circuit Breaker (Polly / Resilience4j).
  2. Fall back to cached ICD-10 rule lookup engine for standard visits.
  3. Queue non-urgent visits into Kafka for background asynchronous processing.

---

## Scenario 2: Cache Invalidation & Data Freshness Tradeoffs

- **Problem**: Coders see outdated patient diagnosis history due to overly aggressive Redis TTL.
- **Solution**: Event-driven cache eviction via CDC (Debezium) listening to database write logs rather than static time-to-live expiration.
