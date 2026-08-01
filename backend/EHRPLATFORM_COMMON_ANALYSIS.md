# EHRPlatform.Common Structure Analysis & Migration Mapping

**Date**: August 1, 2026  
**Total Files**: 328 (mostly .cs files)  
**Purpose**: Map all files from current structure to building-blocks foundation

---

## Layer-by-Layer Breakdown

### 1. DOMAIN LAYER (Pure Business Rules)

**Current Location**: `EHRPlatform.Common/Domain/`

**Target**: `building-blocks/SharedKernel/src/`

#### Files to Move:
```
Domain/
├── Entities/
│   ├── BaseEntity.cs                → SharedKernel/Entities/
│   ├── AuditableEntity.cs           → SharedKernel/Entities/
│   ├── [other entity files]         → SharedKernel/Entities/
│
├── ValueObjects/
│   ├── ValueObject.cs               → SharedKernel/ValueObjects/
│   ├── [value object files]         → SharedKernel/ValueObjects/
│
├── Specifications/
│   ├── Specification.cs             → SharedKernel/Specifications/
│   ├── [specification files]        → SharedKernel/Specifications/
│
├── DomainEvents/
│   ├── DomainEvent.cs               → SharedKernel/DomainEvents/
│   ├── [event files]                → SharedKernel/DomainEvents/
│
├── Enums/
│   └── [enum files]                 → SharedKernel/Enums/
│
├── Exceptions/
│   └── [exception files]            → SharedKernel/Exceptions/
│
└── Constants/
    └── [constant files]             → SharedKernel/Constants/
```

**Namespace Changes**:
```csharp
// Before
namespace EHRPlatform.Common.Domain

// After
namespace EHRPlatform.BuildingBlocks.SharedKernel
```

---

### 2. APPLICATION LAYER (Common Infrastructure)

**Current Location**: `EHRPlatform.Common/Application/Common/`

**Target**: Multiple building-blocks

#### 2.1 CQRS Interfaces

**Files**:
```
Application/Common/CQRS/
├── ICachedQuery.cs                  → EventBus/CQRS/
├── ICommand.cs                      → EventBus/CQRS/
├── ICommandDispatcher.cs            → EventBus/CQRS/
├── IHandler.cs                      → EventBus/CQRS/
├── IQuery.cs                        → EventBus/CQRS/
├── IQueryDispatcher.cs              → EventBus/CQRS/
└── MediatRDispatchers.cs            → EventBus/CQRS/
```

**Namespace**:
```csharp
// Before
namespace EHRPlatform.Common.Application.Common.CQRS

// After
namespace EHRPlatform.BuildingBlocks.EventBus.CQRS
```

#### 2.2 Behaviors (Pipeline)

**Files**:
```
Application/Common/Behaviors/
├── CachingBehavior.cs               → EventBus/Behaviors/
├── LoggingBehavior.cs               → EventBus/Behaviors/
├── TransactionBehavior.cs           → EventBus/Behaviors/
└── ValidationBehavior.cs            → EventBus/Behaviors/
```

**Namespace**:
```csharp
// After
namespace EHRPlatform.BuildingBlocks.EventBus.Behaviors
```

#### 2.3 Extensions

**Files**:
```
Application/Common/Extensions/
├── CQRSExtensions.cs                → EventBus/Extensions/
└── ServiceCollectionExtensions.cs   → EventBus/Extensions/
```

#### 2.4 Mapping

**Files**:
```
Application/Common/Mapping/
├── [AutoMapper profiles]            → Common/Mapping/
└── MappingExtensions.cs             → Common/Extensions/
```

**Target**: `building-blocks/Common/src/`

#### 2.5 Validation

**Files**:
```
Application/Common/Validation/
├── [FluentValidation validators]    → Common/Validation/
```

**Target**: `building-blocks/Common/src/`

---

### 3. INFRASTRUCTURE LAYER (Cross-Cutting Concerns)

**Current Location**: `EHRPlatform.Common/Infrastructure/`

#### 3.1 Caching

**Files**:
```
Infrastructure/Caching/
├── CachingExtensions.cs             → Observability/Caching/
├── CachingServiceExtensions.cs      → Observability/Caching/
├── CacheHealthCheckExtensions.cs    → Observability/Caching/
└── ISlugGenerator.cs / SlugGenerator.cs (uses cache)
```

**Target**: `building-blocks/Observability/src/Caching/`

**Namespace**:
```csharp
// After
namespace EHRPlatform.BuildingBlocks.Observability.Caching
```

#### 3.2 Configuration

**Files**:
```
Infrastructure/Configuration/
├── MongoConfigurationExtensions.cs  → Observability/Configuration/
├── PostgresConfigurationExtensions.cs → Observability/Configuration/
├── MySqlConfigurationExtensions.cs  → Observability/Configuration/
└── MigrationConfigurationExtensions.cs → Observability/Configuration/
```

**Target**: `building-blocks/Observability/src/Configuration/`

#### 3.3 EventDriven / Messaging

**Files**:
```
Infrastructure/EventDriven/
├── MessagingExtensions.cs           → EventBus/Messaging/
├── MassTransitExtensions.cs         → EventBus/Messaging/
├── IntegrationEventExtensions.cs    → EventBus/Messaging/
└── SearchExtensions.cs              → EventBus/Messaging/
```

**Target**: `building-blocks/EventBus/src/Messaging/`

**Namespace**:
```csharp
// After
namespace EHRPlatform.BuildingBlocks.EventBus.Messaging
```

#### 3.4 Health Checks

**Files**:
```
Infrastructure/Health/
├── HealthCheckExtensions.cs         → Observability/HealthChecks/
├── HealthCheckRegistrationExtensions.cs → Observability/HealthChecks/
├── HealthCheckEndpointMappingExtensions.cs → Observability/HealthChecks/
├── ElasticsearchHealthCheck.cs      → Observability/HealthChecks/
├── MongoHealthCheckExtensions.cs    → Observability/HealthChecks/
└── HealthCheckResponseWriters.cs    → Observability/HealthChecks/
```

**Target**: `building-blocks/Observability/src/HealthChecks/`

#### 3.5 Resilience

**Files**:
```
Infrastructure/Resilience/
└── ResilienceExtensions.cs          → Security/Resilience/
```

**Target**: `building-blocks/Security/src/Resilience/`

**Note**: Resilience (retry policies, circuit breakers) relates to security/stability

#### 3.6 Security

**Files**:
```
Infrastructure/Security/
├── JwtExtensions.cs                 → Security/Authentication/
├── SecurityServiceExtensions.cs     → Security/Authentication/
```

**Target**: `building-blocks/Security/src/Authentication/`

**Namespace**:
```csharp
// After
namespace EHRPlatform.BuildingBlocks.Security.Authentication
```

#### 3.7 Telemetry

**Files**:
```
Infrastructure/Telemetry/
├── OpenTelemetryExtensions.cs       → Observability/Telemetry/
└── LoggingServiceExtensions.cs      → Observability/Telemetry/
```

**Target**: `building-blocks/Observability/src/Telemetry/`

**Namespace**:
```csharp
// After
namespace EHRPlatform.BuildingBlocks.Observability.Telemetry
```

---

### 4. DATA LAYER (Persistence)

**Current Location**: `EHRPlatform.Common/Data/`

**Target**: `building-blocks/Common/src/Data/` (Shared configurations only)

#### Files:
```
Data/
├── Contexts/
│   ├── BaseDbContext.cs             → Common/Data/Contexts/
│   ├── EntityTypeConfiguration/
│   │   ├── BaseEntityConfiguration.cs → Common/Data/Contexts/
│   │   └── [other configs]          → Common/Data/Contexts/
│   └── MongoExtensions.cs           → Observability/Data/
│
├── Implementations/
│   ├── DataAccessExtensions.cs      → Common/Data/Implementations/
│   ├── FilterExtensions.cs          → Common/Data/Implementations/
│   ├── MigrationExtensions.cs       → Common/Data/Implementations/
│   └── MongoMigrationExtensions.cs  → Observability/Data/
│
└── Repositories/
    ├── IRepository.cs               → Common/Data/Repositories/
    └── RepositoryBase.cs            → Common/Data/Repositories/
```

**Namespace**:
```csharp
// After
namespace EHRPlatform.BuildingBlocks.Common.Data
```

---

### 5. SHARED LAYER (Utilities & Middleware)

**Current Location**: `EHRPlatform.Common/Shared/`

#### 5.1 Middleware

**Files**:
```
Shared/Middleware/
├── CorrelationIdMiddleware.cs       → Common/Middleware/
├── GlobalExceptionMiddleware.cs     → Common/Middleware/
├── RequestLoggingMiddleware.cs      → Common/Middleware/
└── [Extensions for above]           → Common/Middleware/
```

**Target**: `building-blocks/Common/src/Middleware/`

#### 5.2 Utilities

**Files**:
```
Shared/Utilities/
├── SlugExtensions.cs                → Common/Utilities/
├── SlugValidator.cs                 → Common/Utilities/
├── SlugGenerator.cs                 → Common/Utilities/
├── ArgumentGuard.cs                 → Common/Utilities/
├── CollectionHelper.cs              → Common/Utilities/
├── ConversionHelper.cs              → Common/Utilities/
├── DateTimeHelper.cs                → Common/Utilities/
├── GuidHelper.cs                    → Common/Utilities/
├── JsonSerializationHelper.cs       → Common/Utilities/
├── StringHelper.cs                  → Common/Utilities/
├── CacheKeyGenerator.cs             → Common/Utilities/
└── EHRCommonOptions.cs              → Common/Utilities/
```

**Target**: `building-blocks/Common/src/Utilities/`

#### 5.3 DTOs

**Files**:
```
Shared/DTOs/
├── Pagination/
│   ├── PagedResult.cs               → Contracts/DTOs/
│   ├── PaginationRequest.cs         → Contracts/DTOs/
│   └── [other pagination]           → Contracts/DTOs/
│
├── Responses/
│   ├── ApiResponse.cs               → Contracts/DTOs/
│   ├── ApiResponseGeneric.cs        → Contracts/DTOs/
│   ├── ErrorResponse.cs             → Contracts/DTOs/
│   ├── PagedApiResponse.cs          → Contracts/DTOs/
│   └── ProblemDetails.cs            → Contracts/DTOs/
│
├── Tags/
│   ├── TagDto.cs                    → Contracts/DTOs/
│   ├── [other tag DTOs]             → Contracts/DTOs/
│   └── [Tag contracts/interfaces]   → Contracts/Contracts/
│
├── Cross-Service/
│   ├── AnalyticsDto.cs              → Contracts/DTOs/
│   ├── AppointmentDto.cs            → Contracts/DTOs/
│   ├── AuditDto.cs                  → Contracts/DTOs/
│   ├── BillingDto.cs                → Contracts/DTOs/
│   ├── ClinicalDto.cs               → Contracts/DTOs/
│   ├── NotificationDto.cs           → Contracts/DTOs/
│   ├── PatientDto.cs                → Contracts/DTOs/
│   ├── PrescriptionDto.cs           → Contracts/DTOs/
│   └── UserDto.cs                   → Contracts/DTOs/
│
└── [Other DTOs]
```

**Target**: `building-blocks/Contracts/src/DTOs/`

#### 5.4 Responses & Problem Details

**Files**:
```
Shared/Responses/
├── ProblemDetails.cs                → Contracts/Responses/
```

**Target**: `building-blocks/Contracts/src/Responses/`

#### 5.5 Contracts/Interfaces

**Files**:
```
Shared/Contracts/
├── ICategoryProvider.cs             → Contracts/Contracts/
├── ITagQueryService.cs              → Contracts/Contracts/
└── ITagService.cs                   → Contracts/Contracts/
```

**Target**: `building-blocks/Contracts/src/Contracts/`

#### 5.6 Localization

**Files**:
```
Shared/Localization/
└── LocalizationExtensions.cs        → Common/Localization/
```

**Target**: `building-blocks/Common/src/Localization/`

---

## Migration Mapping Summary Table

| Layer | Current Location | Target Location | Namespace Change |
|-------|------------------|-----------------|------------------|
| **Domain** | Domain/ | SharedKernel/Entities/, Enums/, Exceptions/, Constants/ | EHRPlatform.Common.Domain → EHRPlatform.BuildingBlocks.SharedKernel |
| **CQRS** | Application/Common/CQRS/ | EventBus/CQRS/ | EHRPlatform.Common.Application.Common.CQRS → EHRPlatform.BuildingBlocks.EventBus.CQRS |
| **Behaviors** | Application/Common/Behaviors/ | EventBus/Behaviors/ | EHRPlatform.Common.Application.Common.Behaviors → EHRPlatform.BuildingBlocks.EventBus.Behaviors |
| **Messaging** | Infrastructure/EventDriven/ | EventBus/Messaging/ | EHRPlatform.Common.Infrastructure.EventDriven → EHRPlatform.BuildingBlocks.EventBus.Messaging |
| **Security/Auth** | Infrastructure/Security/ | Security/Authentication/ | EHRPlatform.Common.Infrastructure.Security → EHRPlatform.BuildingBlocks.Security.Authentication |
| **Resilience** | Infrastructure/Resilience/ | Security/Resilience/ | EHRPlatform.Common.Infrastructure.Resilience → EHRPlatform.BuildingBlocks.Security.Resilience |
| **Telemetry** | Infrastructure/Telemetry/ | Observability/Telemetry/ | EHRPlatform.Common.Infrastructure.Telemetry → EHRPlatform.BuildingBlocks.Observability.Telemetry |
| **Health** | Infrastructure/Health/ | Observability/HealthChecks/ | EHRPlatform.Common.Infrastructure.Health → EHRPlatform.BuildingBlocks.Observability.HealthChecks |
| **Caching** | Infrastructure/Caching/ | Observability/Caching/ | EHRPlatform.Common.Infrastructure.Caching → EHRPlatform.BuildingBlocks.Observability.Caching |
| **Configuration** | Infrastructure/Configuration/ | Observability/Configuration/ | EHRPlatform.Common.Infrastructure.Configuration → EHRPlatform.BuildingBlocks.Observability.Configuration |
| **Data/Persistence** | Data/ | Common/Data/ | EHRPlatform.Common.Data → EHRPlatform.BuildingBlocks.Common.Data |
| **Mapping** | Application/Common/Mapping/ | Common/Mapping/ | EHRPlatform.Common.Application.Common.Mapping → EHRPlatform.BuildingBlocks.Common.Mapping |
| **Validation** | Application/Common/Validation/ | Common/Validation/ | EHRPlatform.Common.Application.Common.Validation → EHRPlatform.BuildingBlocks.Common.Validation |
| **Middleware** | Shared/Middleware/ | Common/Middleware/ | EHRPlatform.Common.Shared.Middleware → EHRPlatform.BuildingBlocks.Common.Middleware |
| **Utilities** | Shared/Utilities/ | Common/Utilities/ | EHRPlatform.Common.Shared.Utilities → EHRPlatform.BuildingBlocks.Common.Utilities |
| **DTOs** | Shared/DTOs/ | Contracts/DTOs/ | EHRPlatform.Common.Shared.DTOs → EHRPlatform.BuildingBlocks.Contracts.DTOs |
| **Contracts** | Shared/Contracts/ | Contracts/Contracts/ | EHRPlatform.Common.Shared.Contracts → EHRPlatform.BuildingBlocks.Contracts.Contracts |

---

## Building-Blocks Target Structure

```
building-blocks/
│
├── SharedKernel/
│   ├── src/
│   │   ├── SharedKernel.csproj
│   │   ├── Entities/
│   │   ├── ValueObjects/
│   │   ├── Aggregates/
│   │   ├── DomainEvents/
│   │   ├── Specifications/
│   │   ├── Enums/
│   │   ├── Exceptions/
│   │   ├── Constants/
│   │   └── Interfaces/
│   └── tests/
│
├── EventBus/
│   ├── src/
│   │   ├── EventBus.csproj
│   │   ├── CQRS/
│   │   ├── Behaviors/
│   │   ├── Messaging/
│   │   ├── Extensions/
│   │   └── Interfaces/
│   └── tests/
│
├── Security/
│   ├── src/
│   │   ├── Security.csproj
│   │   ├── Authentication/
│   │   ├── Authorization/
│   │   ├── Resilience/
│   │   ├── Encryption/
│   │   └── Extensions/
│   └── tests/
│
├── Observability/
│   ├── src/
│   │   ├── Observability.csproj
│   │   ├── Telemetry/
│   │   ├── HealthChecks/
│   │   ├── Caching/
│   │   ├── Configuration/
│   │   ├── Logging/
│   │   ├── Metrics/
│   │   └── Extensions/
│   └── tests/
│
├── Common/
│   ├── src/
│   │   ├── Common.csproj
│   │   ├── Data/
│   │   ├── Mapping/
│   │   ├── Validation/
│   │   ├── Middleware/
│   │   ├── Utilities/
│   │   ├── Localization/
│   │   ├── Extensions/
│   │   └── Constants/
│   └── tests/
│
└── Contracts/
    ├── src/
    │   ├── Contracts.csproj
    │   ├── DTOs/
    │   ├── Responses/
    │   ├── Contracts/
    │   ├── Events/
    │   ├── Enums/
    │   └── Constants/
    └── tests/
```

---

## Key Points for Migration

### ✅ DO:
- Move files with updated namespaces
- Keep logical grouping by concern
- Update all `.csproj` references
- Create .csproj files for each building-block
- Update service references after building-blocks are complete

### ❌ DON'T:
- Duplicate files
- Copy instead of move
- Leave old structure after migration
- Forget namespace updates
- Skip updating service references

---

## Next Steps

1. Create physical directory structure for building-blocks
2. Move files by layer to new locations
3. Update namespaces in moved files
4. Create .csproj files for each building-block
5. Update all service references
6. Verify build passes
7. Commit to git

