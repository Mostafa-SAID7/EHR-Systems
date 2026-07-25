# Common Interview Questions & Prepared Answers

---

## 1. "How do you handle technical disagreements on architecture?"

**Answer Script:**
> "I focus on data, benchmarks, and rapid prototyping rather than opinions. For example, at We3ds when deciding between synchronous REST calls vs Kafka event streaming for payment reconciliation, I built a quick prototype comparing throughput under high concurrency. Showing concrete numbers helped aligned the team on an event-driven approach."

---

## 2. "How do you ensure data accuracy when scaling database writes?"

**Answer Script:**
> "I balance write performance with strict consistency using optimistic concurrency control, proper indexing, and avoiding distributed transactions where possible by applying the Outbox and Saga patterns."
