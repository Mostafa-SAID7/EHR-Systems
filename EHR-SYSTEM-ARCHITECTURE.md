# Enterprise-Grade Healthcare Microservices Architecture

## Directory Structure (Target)

```
EHR-System/
│
├── gateway/
│   ├── ApiGateway/
│   │   ├── src/
│   │   ├── tests/
│   │   └── docker/
│   └── BFF/
│       ├── src/
│       ├── tests/
│       └── docker/
│
├── building-blocks/
│   ├── EventBus/
│   │   ├── src/
│   │   └── tests/
│   ├── SharedKernel/
│   │   ├── src/
│   │   └── tests/
│   ├── Contracts/
│   │   └── src/
│   ├── Security/
│   │   ├── src/
│   │   └── tests/
│   ├── Observability/
│   │   ├── src/
│   │   └── tests/
│   └── Common/
│       ├── src/
│       └── tests/
│
├── services/
│   ├── Identity/
│   │   ├── src/
│   │   │   ├── Identity.API/
│   │   │   ├── Identity.Application/
│   │   │   ├── Identity.Domain/
│   │   │   ├── Identity.Infrastructure/
│   │   │   ├── Identity.Persistence/
│   │   │   └── Identity.Contracts/
│   │   ├── tests/
│   │   └── docker/
│   │
│   ├── Patient/
│   │   ├── src/
│   │   │   ├── Patient.API/
│   │   │   ├── Patient.Application/
│   │   │   ├── Patient.Domain/
│   │   │   ├── Patient.Infrastructure/
│   │   │   ├── Patient.Persistence/
│   │   │   └── Patient.Contracts/
│   │   ├── tests/
│   │   └── docker/
│   │
│   ├── Practitioner/
│   ├── Appointment/
│   ├── Encounter/
│   ├── MedicalRecord/
│   ├── Laboratory/
│   ├── Radiology/
│   ├── Pharmacy/
│   ├── Billing/
│   ├── Insurance/
│   ├── Notification/
│   ├── Audit/
│   ├── FileStorage/
│   ├── Terminology/
│   ├── AI/
│   └── Integration/
│
├── infrastructure/
│   ├── Logging/
│   ├── Caching/
│   ├── Authentication/
│   ├── Persistence/
│   ├── Messaging/
│   ├── BlobStorage/
│   └── Monitoring/
│
├── deployment/
│   ├── docker/
│   ├── kubernetes/
│   ├── terraform/
│   └── github-actions/
│
└── docs/
    ├── architecture/
    ├── api/
    ├── deployment/
    └── migration-guide/
```

## Service Architecture (5-Layer Pattern per Service)

Each service follows this internal structure:

```
PatientService/
├── src/
│   ├── Patient.API/
│   │   ├── Controllers/
│   │   ├── Middleware/
│   │   ├── Extensions/
│   │   └── Program.cs
│   │
│   ├── Patient.Application/
│   │   ├── Commands/
│   │   ├── Queries/
│   │   ├── Services/
│   │   ├── Mappers/
│   │   └── DependencyInjection.cs
│   │
│   ├── Patient.Domain/
│   │   ├── Entities/
│   │   ├── ValueObjects/
│   │   ├── Aggregates/
│   │   ├── DomainEvents/
│   │   └── Specifications/
│   │
│   ├── Patient.Infrastructure/
│   │   ├── Integration/
│   │   ├── ExternalServices/
│   │   ├── FHIR/
│   │   └── DependencyInjection.cs
│   │
│   ├── Patient.Persistence/
│   │   ├── Data/
│   │   ├── DbContext/
│   │   ├── Repositories/
│   │   ├── Migrations/
│   │   └── DependencyInjection.cs
│   │
│   └── Patient.Contracts/
│       ├── Requests/
│       ├── Responses/
│       ├── Events/
│       ├── DTOs/
│       ├── Enums/
│       └── Constants/
│
├── tests/
│   ├── Patient.Application.Tests/
│   ├── Patient.Domain.Tests/
│   ├── Patient.Integration.Tests/
│   └── Patient.Api.Tests/
│
└── docker/
    ├── Dockerfile
    └── docker-compose.yml
```

## Building Blocks (Shared Infrastructure)

### EventBus
- Kafka/RabbitMQ abstraction
- Event publishing & subscribing
- Dead letter queue handling
- Retry policies

### SharedKernel
- Base entities & aggregates
- Domain events
- Value objects
- Specifications
- CQRS interfaces

### Contracts
- No shared DTOs (each service has own Contracts project)
- Only defines interfaces for cross-service communication
- Event definitions (used by all services)
- gRPC protos
- FHIR resource definitions

### Security
- JWT token validation
- RBAC enforcement
- ABAC policies
- Encryption utilities

### Observability
- OpenTelemetry integration
- Structured logging (Serilog)
- Health checks
- Metrics collection
- Correlation IDs

### Common
- Extension methods
- Constants
- Utilities
- Configuration helpers

## Healthcare-Specific Services

### Integration Service
Routes all external system integrations:
- HL7 v2 conversions
- FHIR resource mapping
- NPHIES (Saudi Arabia insurance)
- Payment gateway integration
- Government APIs
- Email/SMS routing

### Terminology Service
Manages clinical terminology:
- ICD-10 codes
- SNOMED codes
- LOINC codes
- CPT codes
- RXNorm
- Custom terminology mappings

### Audit Service
Immutable audit logging:
- All data modifications
- Patient access logs
- Security incidents
- Export capabilities
- 7-year retention

### FileStorage Service
Document & media management:
- Upload/download
- Virus scanning
- Version control
- Metadata extraction
- Lifecycle management

### AI Service
Separate AI/ML capabilities:
- Clinical predictions
- Medical coding recommendations
- Fraud detection
- Risk scoring
- LLM integration
- RAG (Retrieval Augmented Generation)

## Key Principles

### 1. No Shared Domain Models
- Each service owns its entities
- Communication via Contracts (events, DTOs, gRPC)
- Patient entity only exists in Patient Service
- Other services have read-only cache

### 2. Database-per-Service
- Patient Service: PostgreSQL + MongoDB
- Appointment Service: PostgreSQL
- Billing Service: MySQL
- Audit Service: Immutable log store
- No cross-service foreign keys

### 3. Event-Driven Communication
- Services publish domain events
- Other services subscribe
- Outbox/Inbox pattern for reliability
- Kafka or RabbitMQ as message bus

### 4. API Contracts
- Each service: REST API (HTTP)
- Optional: gRPC for internal calls
- FHIR compliance where applicable
- OpenAPI documentation

### 5. Dependency Flow
```
API Layer (never depends on anything else)
    ↓ (depends on)
Application Layer (CQRS, business logic)
    ↓ (depends on)
Domain Layer (pure business rules, no frameworks)
    ↓ (depends on)
Infrastructure & Persistence (external services, DB)
```

Domain never depends on Infrastructure.
Application never depends on Infrastructure directly (only interfaces).
API depends on everything (orchestrates).

## Migration Path

### Phase 1-2: Build New Structure
- Create building-blocks packages
- Create new service folders
- No code changes yet

### Phase 3-5: Migrate Services
- Copy existing code to new structure
- Reorganize into 5-layer pattern
- Update namespaces
- Update dependencies

### Phase 6-8: Add Healthcare Services
- Implement missing services
- Add Integration service
- Add Terminology service
- Add Audit service

### Phase 9-10: Cleanup
- Remove old structure
- Update documentation
- Migration guide for teams

## Key Metrics (Target)

- ✅ 16 independent microservices
- ✅ 0 shared domain models (each service owns entities)
- ✅ 100% event-driven communication
- ✅ FHIR-compliant APIs
- ✅ HIPAA audit compliance
- ✅ Immutable audit logs
- ✅ Integration service for all external systems
- ✅ Terminology service for all clinical codes
- ✅ AI service for predictions & recommendations

## Success Criteria

| Criterion | Current | Target |
|-----------|---------|--------|
| Services | 10 | 16 |
| Domain model sharing | ❌ YES | ✅ NO |
| Building blocks | ❌ Partial | ✅ Complete |
| Service contracts | ❌ In Application | ✅ Separate project |
| Healthcare services | ❌ No | ✅ Yes |
| Integration service | ❌ No | ✅ Yes |
| Terminology service | ❌ No | ✅ Yes |
| Audit service | ❌ Partial | ✅ Complete |
| FileStorage service | ❌ No | ✅ Yes |
| AI service | ❌ No | ✅ Yes |
| FHIR compliance | ❌ No | ✅ Yes |

