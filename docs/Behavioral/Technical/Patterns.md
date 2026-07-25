# Distributed Architecture Patterns

Core backend patterns applied in high-reliability e-commerce (We3ds) and healthcare platforms (TachyHealth).

---

## 1. CQRS (Command Query Responsibility Segregation)

- **Write Path (Command)**: Strictly normalized PostgreSQL tables for patient visit state mutations and code assignments.
- **Read Path (Query)**: Denormalized Redis/Elasticsearch read views for sub-100ms dashboard filtering and reporting.

---

## 2. Event Sourcing & Kafka Event Bus

- Record state changes as an immutable sequence of events (`VisitCreated`, `CodeSuggested`, `CodeConfirmed`, `ClaimSubmitted`).
- Allows historical audit replay and compliance verification.

---

## 3. Transactional Outbox Pattern

- Guarantees event delivery when updating local databases.
- Writes entity changes AND outgoing message payload into the same database transaction before Kafka worker consumption.

---

## 4. Saga Pattern (Distributed Transactions)

- Coordinates multi-step billing and insurance workflows without distributed 2-phase commit locks.
- Uses compensating actions (e.g., `CancelClaimSubmission`) if downstream insurance verification fails.
