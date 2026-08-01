# Building Blocks

Enterprise-grade abstractions for EHR System. 205 files, 100% SRP compliant.

## Packages

### [Common](./Common/README.md)
Cross-cutting concerns: Resilience, Background Jobs, Search, File Storage, Serialization, Caching, Validation.

### [SharedKernel](./SharedKernel/README.md)
Domain patterns: CQRS, Event Sourcing, Repositories, Domain Model, Specifications, Result Pattern.

### [Contracts](./Contracts/README.md)
API contracts: Request/Response envelopes, DTOs, Pagination, Error handling.

### [EventBus](./EventBus/README.md)
Event communication: Integration Events, Domain Events, Message Broker, Outbox Pattern.

### [Observability](./Observability/README.md)
Monitoring & Tracing: Health Checks, Logging, Telemetry, Performance, Distributed Tracing.

### [Security](./Security/README.md)
Security patterns: Authentication, Authorization, Multi-Tenancy, Encryption, Audit Logging.

---

## Status

✅ **205 files** - 100% SRP compliant, 0 duplicates  
✅ **TIER 1** - Resilience, Event Bus, Tracing, Error Reporting, Background Jobs  
✅ **TIER 2** - Multi-Tenancy, Event Sourcing, Outbox, Search, File Storage

## Quick Links

- [Common Package](./Common/README.md)
- [SharedKernel Package](./SharedKernel/README.md)
- [Contracts Package](./Contracts/README.md)
- [EventBus Package](./EventBus/README.md)
- [Observability Package](./Observability/README.md)
- [Security Package](./Security/README.md)

## Architecture

Each package follows consistent structure:

```
PackageName/
├── src/
│   ├── Category1/
│   ├── Category2/
│   └── ...
└── tests/
```

All files follow **1 class/interface per file** rule for maintainability.
