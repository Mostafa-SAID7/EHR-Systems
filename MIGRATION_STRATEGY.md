# EHR System Architecture Migration Strategy

**Objective**: Migrate from current monolithic/old structure to Enterprise-Grade Healthcare Microservices Architecture without code duplication.

**Approach**: Move & Reorganize (NOT Copy)

---

## Current Structure Analysis

### Current Backend Organization
```
backend/src/
├── EHRPlatform.Common/           (Shared infrastructure - KEEP & REORGANIZE)
├── EHRPlatform.Services.*        (Individual services - REORGANIZE)
├── EHRPlatform.Services.Analytics
├── EHRPlatform.Services.Appointment
├── EHRPlatform.Services.Audit
├── EHRPlatform.Services.Billing
├── EHRPlatform.Services.Clinical
├── EHRPlatform.Services.Identity
├── EHRPlatform.Services.Notification
├── EHRPlatform.Services.OutboxProcessor
├── EHRPlatform.Services.Patient
└── EHRPlatform.Services.Prescription
```

### Issues with Current Structure
- ❌ Services don't follow consistent 5-layer pattern
- ❌ Common layer mixed with application logic
- ❌ No separation between Persistence and Domain
- ❌ No dedicated Infrastructure layer per service
- ❌ Contracts mixed with Application
- ❌ No building-blocks separation
- ❌ No API layer clearly defined

---

## Target Structure

### New Architecture
```
EHR-System/
├── building-blocks/              (Centralized shared packages)
│   ├── EventBus/
│   ├── SharedKernel/
│   ├── Contracts/
│   ├── Security/
│   ├── Observability/
│   └── Common/
│
├── services/
│   ├── Identity/
│   ├── Patient/
│   ├── Appointment/
│   ├── Billing/
│   ├── Audit/
│   ├── Notification/
│   ├── Integration/               (NEW - external integrations)
│   ├── Terminology/               (NEW - clinical codes)
│   ├── FileStorage/               (NEW - document management)
│   ├── AI/                        (NEW - ML/AI capabilities)
│   └── [other services]/
│
├── gateway/
│   ├── ApiGateway/
│   └── BFF/
│
├── infrastructure/
├── deployment/
└── docs/
```

---

## Migration Phases

### PHASE 1: Prepare Building Blocks (Week 1-2)
**Goal**: Create foundation without touching services

#### 1.1 Create building-blocks directory structure
```
building-blocks/
├── EventBus/
│   ├── src/
│   │   ├── EventBus.csproj
│   │   ├── IEventBus.cs
│   │   ├── EventHandler.cs
│   │   └── [move from Common/Infrastructure/EventDriven/]
│   └── tests/
│
├── SharedKernel/
│   ├── src/
│   │   ├── SharedKernel.csproj
│   │   ├── Entity.cs
│   │   ├── Aggregate.cs
│   │   ├── DomainEvent.cs
│   │   ├── ValueObject.cs
│   │   ├── Specification.cs
│   │   └── [move from Common/Domain/]
│   └── tests/
│
├── Contracts/
│   ├── src/
│   │   ├── Contracts.csproj
│   │   ├── Events/
│   │   ├── DTOs/
│   │   ├── Enums/
│   │   └── Constants/
│   └── tests/
│
├── Security/
│   ├── src/
│   │   ├── Security.csproj
│   │   ├── [move from Common/Infrastructure/Security/]
│   │   └── Authentication/
│   └── tests/
│
├── Observability/
│   ├── src/
│   │   ├── Observability.csproj
│   │   ├── [move from Common/Infrastructure/Telemetry/]
│   │   ├── Logging/
│   │   ├── Tracing/
│   │   ├── Metrics/
│   │   └── HealthChecks/
│   └── tests/
│
└── Common/
    ├── src/
    │   ├── Common.csproj
    │   ├── [move from Common/Shared/ and Common/Application/Common/Extensions/]
    │   ├── Extensions/
    │   ├── Utilities/
    │   └── Constants/
    └── tests/
```

#### 1.2 Create service template structure
```
services/
└── ServiceTemplate/
    ├── src/
    │   ├── ServiceName.API/
    │   │   ├── ServiceName.API.csproj
    │   │   ├── Controllers/
    │   │   ├── Middleware/
    │   │   ├── Filters/
    │   │   ├── Extensions/
    │   │   └── Program.cs
    │   │
    │   ├── ServiceName.Application/
    │   │   ├── ServiceName.Application.csproj
    │   │   ├── Features/
    │   │   │   └── FeatureName/
    │   │   │       ├── Commands/
    │   │   │       ├── Queries/
    │   │   │       └── Validators/
    │   │   ├── Services/
    │   │   ├── Mappers/
    │   │   └── DependencyInjection.cs
    │   │
    │   ├── ServiceName.Domain/
    │   │   ├── ServiceName.Domain.csproj
    │   │   ├── Entities/
    │   │   ├── ValueObjects/
    │   │   ├── Aggregates/
    │   │   ├── DomainEvents/
    │   │   ├── Specifications/
    │   │   └── Enums/
    │   │
    │   ├── ServiceName.Infrastructure/
    │   │   ├── ServiceName.Infrastructure.csproj
    │   │   ├── Integration/
    │   │   ├── ExternalServices/
    │   │   ├── FHIR/
    │   │   └── DependencyInjection.cs
    │   │
    │   ├── ServiceName.Persistence/
    │   │   ├── ServiceName.Persistence.csproj
    │   │   ├── DbContext/
    │   │   ├── Repositories/
    │   │   ├── Migrations/
    │   │   ├── Configurations/
    │   │   └── DependencyInjection.cs
    │   │
    │   └── ServiceName.Contracts/
    │       ├── ServiceName.Contracts.csproj
    │       ├── Requests/
    │       ├── Responses/
    │       ├── DTOs/
    │       └── Events/
    │
    ├── tests/
    │   ├── ServiceName.Application.Tests/
    │   ├── ServiceName.Domain.Tests/
    │   ├── ServiceName.Integration.Tests/
    │   └── ServiceName.API.Tests/
    │
    └── docker/
        ├── Dockerfile
        └── docker-compose.override.yml
```

**Deliverable**: Empty project structure ready for code

---

### PHASE 2: Migrate EHRPlatform.Common → building-blocks (Week 2-3)

#### 2.1 Move Domain Layer → SharedKernel
**From**: `EHRPlatform.Common/Domain/`
**To**: `building-blocks/SharedKernel/src/`

Move:
- ✓ `Domain/Entities/BaseEntity.cs`
- ✓ `Domain/Entities/AuditableEntity.cs`
- ✓ `Domain/Entities/ValueObject.cs`
- ✓ `Domain/Enums/*`
- ✓ `Domain/Exceptions/*`
- ✓ `Domain/Specifications/*`
- ✓ `Domain/Constants/*`

Action: Direct move, update namespace to `EHRPlatform.BuildingBlocks.SharedKernel`

#### 2.2 Move Infrastructure → building-blocks
**EventDriven** → `building-blocks/EventBus/src/`
- ✓ Move `Infrastructure/EventDriven/*.cs`

**Security** → `building-blocks/Security/src/`
- ✓ Move `Infrastructure/Security/*.cs`

**Observability** → `building-blocks/Observability/src/`
- ✓ Move `Infrastructure/Telemetry/`
- ✓ Move `Infrastructure/Health/`

**Common** → `building-blocks/Common/src/`
- ✓ Move `Shared/Utilities/`
- ✓ Move `Shared/Middleware/`
- ✓ Move `Shared/Extensions/` (non-service-specific)

#### 2.3 Create Contracts → building-blocks
**From**: Merge from all services + new

**To**: `building-blocks/Contracts/src/`

Create:
- ✓ `Events/` - All domain events
- ✓ `DTOs/` - Global DTOs
- ✓ `Enums/` - Shared enums
- ✓ `Constants/` - Global constants

**Deliverable**: building-blocks fully functional, EHRPlatform.Common now lightweight

---

### PHASE 3: Migrate Individual Services (Week 3-6)

#### 3.1 Pattern: Identity Service Migration

**OLD Structure**:
```
EHRPlatform.Services.Identity/
├── Application/
├── Controllers/
├── Data/
├── Domain/
├── Infrastructure/
├── [mixed layers]
└── Program.cs
```

**NEW Structure**:
```
services/Identity/
├── src/
│   ├── Identity.API/
│   │   ├── Controllers/          [move from old Controllers/]
│   │   ├── Middleware/           [move from old Infrastructure/Middleware/]
│   │   ├── Filters/
│   │   ├── Extensions/           [move from old Extensions/]
│   │   └── Program.cs            [refactor]
│   │
│   ├── Identity.Application/
│   │   ├── Features/
│   │   │   ├── Users/
│   │   │   │   ├── Commands/    [move from old Application/Commands/]
│   │   │   │   ├── Queries/     [move from old Application/Queries/]
│   │   │   │   └── Validators/
│   │   │   └── Authentication/
│   │   ├── Services/             [move from old Application/Services/]
│   │   ├── Mappers/              [move from old Application/Mapping/]
│   │   └── DependencyInjection.cs
│   │
│   ├── Identity.Domain/
│   │   ├── Entities/             [move from old Domain/Entities/]
│   │   ├── ValueObjects/         [move from old Domain/ValueObjects/]
│   │   ├── Aggregates/           [move or create]
│   │   ├── DomainEvents/         [move from old Events/]
│   │   └── Specifications/       [move from old Specifications/]
│   │
│   ├── Identity.Infrastructure/
│   │   ├── Integration/          [move from old Integration/]
│   │   ├── ExternalServices/     [move from old Services/]
│   │   ├── FHIR/                 [create if needed]
│   │   └── DependencyInjection.cs
│   │
│   ├── Identity.Persistence/     [NEW - split from old Data/]
│   │   ├── DbContext/            [move from old Data/Contexts/]
│   │   ├── Repositories/         [move from old Data/Repositories/]
│   │   ├── Configurations/       [move from old Data/Configuration/]
│   │   ├── Migrations/           [move from old Data/Migrations/]
│   │   └── DependencyInjection.cs
│   │
│   └── Identity.Contracts/       [NEW - split from Application]
│       ├── Requests/             [move from old DTO/Requests/]
│       ├── Responses/            [move from old DTO/Responses/]
│       ├── Events/               [move from old Events/]
│       └── DTOs/
│
├── tests/                         [reorganize]
│   ├── Identity.Application.Tests/
│   ├── Identity.Domain.Tests/
│   ├── Identity.Integration.Tests/
│   └── Identity.API.Tests/
│
└── docker/
    ├── Dockerfile               [move from old]
    └── docker-compose.override.yml
```

**Migration Steps for Each Service**:

1. **Create new project structure** (empty)
2. **Move code by layer**:
   - Domain → Identity.Domain
   - Application → Identity.Application
   - Data/Persistence → Identity.Persistence (NEW)
   - Infrastructure → Identity.Infrastructure
   - Controllers → Identity.API
   - Contracts → Identity.Contracts (NEW)
3. **Update namespaces** in moved files
4. **Update project references** (.csproj)
5. **Update DependencyInjection.cs** per layer
6. **Update Program.cs** to use new DI
7. **Run tests** to verify functionality
8. **Delete old projects**

#### 3.2 Services to Migrate (Priority Order)

**Priority 1** (Core):
- [ ] Identity Service
- [ ] Patient Service
- [ ] Appointment Service

**Priority 2** (Business):
- [ ] Billing Service
- [ ] Clinical Service
- [ ] Prescription Service

**Priority 3** (Supporting):
- [ ] Notification Service
- [ ] Audit Service
- [ ] Analytics Service

**Priority 4** (New):
- [ ] Integration Service (NEW)
- [ ] Terminology Service (NEW)
- [ ] FileStorage Service (NEW)
- [ ] AI Service (NEW)

**Deliverable**: All services follow 5-layer pattern

---

### PHASE 4: Create Missing Healthcare Services (Week 6-8)

#### 4.1 Integration Service
**Purpose**: Route all external integrations

Includes:
- HL7 v2 converter
- FHIR mapper
- NPHIES (Saudi insurance)
- Payment gateway
- Government APIs
- Email/SMS provider

#### 4.2 Terminology Service
**Purpose**: Manage clinical terminology

Includes:
- ICD-10 browser
- SNOMED code lookups
- LOINC codes
- CPT codes
- RXNorm
- Custom mappings

#### 4.3 FileStorage Service
**Purpose**: Document & media management

Includes:
- Upload/download
- Virus scanning
- Versioning
- Metadata extraction
- Lifecycle management

#### 4.4 AI Service
**Purpose**: ML/AI capabilities

Includes:
- Clinical predictions
- Medical coding recommendations
- Fraud detection
- Risk scoring
- LLM integration
- RAG

**Deliverable**: 4 new complete services

---

### PHASE 5: Create Gateways & Routing (Week 8-9)

#### 5.1 API Gateway
- Route to services
- Rate limiting
- API versioning
- CORS

#### 5.2 BFF (Backend for Frontend)
- Aggregation
- Response shaping
- Business logic for UI

**Deliverable**: Gateway layer functional

---

### PHASE 6: Update Deployment (Week 9-10)

#### 6.1 Docker Setup
- New Dockerfile per service
- docker-compose for local dev

#### 6.2 Kubernetes
- Helm charts
- Service meshes
- Ingress rules

#### 6.3 GitHub Actions
- Build pipelines
- Test automation
- Deployment workflows

**Deliverable**: Full deployment automation

---

### PHASE 7: Documentation & Cleanup (Week 10-11)

#### 7.1 Documentation
- Architecture diagrams
- API docs (OpenAPI)
- Deployment guide
- Migration guide

#### 7.2 Cleanup
- Remove old structure
- Update all references
- Verify no orphaned code
- Clean up NuGet packages

**Deliverable**: Clean, documented codebase

---

## Key Principles During Migration

### ✅ DO:
- Move code (don't copy)
- Update namespaces in moved files
- Verify tests pass after each move
- Commit changes after each service migration
- Keep old and new side-by-side during migration
- Update documentation

### ❌ DON'T:
- Duplicate code
- Create parallel structures
- Leave old code after migration
- Break tests
- Skip namespace updates
- Migrate without tests

---

## Verification Checklist

### Per Service Migration:
- [ ] New project structure created
- [ ] Code moved to correct layers
- [ ] Namespaces updated
- [ ] Project references updated
- [ ] DependencyInjection.cs created per layer
- [ ] Program.cs refactored for new structure
- [ ] All tests passing
- [ ] No code duplication
- [ ] Old projects deleted
- [ ] Committed to git

### Overall Migration:
- [ ] All 10 services migrated
- [ ] 4 new services created
- [ ] building-blocks complete
- [ ] Gateways functional
- [ ] Deployment updated
- [ ] Documentation complete
- [ ] Zero code duplication
- [ ] All tests passing
- [ ] Build successful

---

## Rollback Strategy

If issues occur:
- Git allows easy rollback per commit
- Keep old structure in branch until migration proven
- Test each phase before proceeding
- Maintain working version on `main`

---

## Timeline

| Phase | Duration | Deliverable |
|-------|----------|------------|
| 1 | Week 1-2 | building-blocks structure |
| 2 | Week 2-3 | EHRPlatform.Common → building-blocks |
| 3 | Week 3-6 | Migrate 10 services |
| 4 | Week 6-8 | Create 4 new services |
| 5 | Week 8-9 | Gateways & routing |
| 6 | Week 9-10 | Deployment setup |
| 7 | Week 10-11 | Documentation & cleanup |

**Total**: ~11 weeks

---

## Success Criteria

✅ No code duplication  
✅ All services follow 5-layer pattern  
✅ building-blocks fully functional  
✅ All tests passing  
✅ FHIR compliance  
✅ Event-driven communication working  
✅ Complete documentation  
✅ Deployment automated  
✅ Clean git history  
