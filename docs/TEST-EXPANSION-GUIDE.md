# Test Expansion Guide - From Reference to All 11 Services

## Overview

This guide explains how to expand the reference test implementations (PatientService, AppointmentService, BillingService, AuditService) to all 11 microservices in the EHR platform.

## 11 Microservices to Test

1. ✅ **Identity Service** - Authentication, authorization, user management
2. ✅ **Patient Service** - Patient records, demographics
3. ✅ **Clinical Service** - Clinical notes, diagnoses, treatments
4. ✅ **Appointment Service** - Scheduling, availability
5. ✅ **Billing Service** - Invoicing, payments, insurance
6. ✅ **Prescription Service** - Medication management
7. ✅ **Notification Service** - Email, SMS, push notifications
8. ✅ **Audit Service** - Compliance logging, access tracking
9. ✅ **Analytics Service** - Reporting, dashboards
10. ✅ **OutboxProcessor Service** - Event processing, saga orchestration
11. ✅ **ApiGateway Service** - API routing, rate limiting

## Testing Pattern Applied

Each service follows this pattern:

### 1. Unit Tests (`EHRPlatform.Tests.Unit/`)
- **20-30 tests per service**
- Test domain entities, validators, business logic
- 100% mocked dependencies
- File: `Application/{ServiceName}ValidatorTests.cs`
- File: `Services/{ServiceName}ServiceTests.cs`
- Example: `PatientValidatorTests.cs` (20 tests), `PatientServiceTests.cs` (10 tests)

### 2. Integration Tests (`EHRPlatform.Tests.Integration/`)
- **10-15 tests per service**
- Test with real database (Testcontainers)
- Cache layer integration
- Repository patterns
- File: `{ServiceName}/{ServiceName}IntegrationTests.cs`
- Example: `PatientService/PatientRepositoryIntegrationTests.cs`, `PatientServiceIntegrationTests.cs`

### 3. Security Tests (`EHRPlatform.Tests.Security/`)
- **10-20 tests per service**
- HIPAA compliance checks
- Authentication/Authorization
- Data protection (PHI encryption)
- Audit logging
- Example: `DataProtection/PhiProtectionTests.cs`, `Authentication/JwtTokenTests.cs`

## Step-by-Step Expansion Guide

### Step 1: Analyze Service Domain

For each service (e.g., `ClinicalService`):

1. Read domain entities in `backend/src/EHRPlatform.Services.{ServiceName}/Domain/Entities/`
2. Identify main entities and relationships
3. Note business rules and constraints

Example - ClinicalService:
```
Entities:
- ClinicalNote
- Diagnosis
- MedicalRecord
- Treatment

Key Relationships:
- Patient has many ClinicalNotes
- ClinicalNote has many Diagnoses
```

### Step 2: Create Unit Test Templates

Copy and adapt `PatientValidatorTests.cs`:

```csharp
namespace EHRPlatform.Tests.Unit.Application;

public class {ServiceName}ValidatorTests
{
    [Fact]
    public void Entity_WithAllRequiredFields_IsValid()
    {
        // Test entity creation with all fields
    }

    [Theory]
    [InlineData(null)]
    public void Entity_WithMissingRequired_IsInvalid(string field)
    {
        // Test validation rules
    }
    
    // ... 15-20 similar tests
}
```

### Step 3: Create Service Tests

Copy and adapt `PatientServiceTests.cs`:

```csharp
namespace EHRPlatform.Tests.Unit.Services;

public class {ServiceName}ServiceTests : UnitTestBase
{
    [Fact]
    public async Task CreateEntity_WithValidData_ReturnsId()
    {
        // CRUD operation test with mocks
    }

    [Fact]
    public async Task UpdateEntity_WithChanges_Persists()
    {
        // Update logic test
    }

    [Fact]
    public async Task Entity_WithCaching_ReturnsCached()
    {
        // Cache behavior test
    }
    
    // ... 8-12 tests covering main operations
}
```

### Step 4: Create Integration Tests

Create new file: `backend/tests/EHRPlatform.Tests.Integration/{ServiceName}/{ServiceName}IntegrationTests.cs`

```csharp
namespace EHRPlatform.Tests.Integration.{ServiceName};

public class {ServiceName}IntegrationTests : IntegrationTestBase
{
    [Fact]
    public async Task Create_WithValidData_Persists()
    {
        // Use real database via IntegrationTestBase
        // Test full workflow with cache
    }

    [Fact]
    public async Task Query_WithIndex_IsPerformant()
    {
        // Performance verification < 100ms
    }
    
    // ... 10-15 tests covering workflows
}
```

### Step 5: Add Security Tests

Create new files in `backend/tests/EHRPlatform.Tests.Security/`:

For each service, add:
- `Authentication/{ServiceName}AuthTests.cs` - JWT, authorization
- `DataProtection/{ServiceName}PhiTests.cs` - Encryption, masking
- `Audit/{ServiceName}AuditTests.cs` - Compliance logging

Example:

```csharp
namespace EHRPlatform.Tests.Security.DataProtection;

public class ClinicalServicePhiTests
{
    [Fact]
    public void ClinicalNotes_IsPhiField()
    {
        // Verify notes are encrypted
        HipaaComplianceHelper.IsPHIField("clinical_notes").Should().BeTrue();
    }

    [Fact]
    public void DiagnosisData_IsEncrypted()
    {
        // Test encryption of sensitive data
    }
    
    [Fact]
    public void AccessToClinicalData_IsAudited()
    {
        // Verify audit trail
    }
}
```

## Test Coverage Targets by Service

| Service | Unit (%) | Integration (%) | Security (%) | Total Tests |
|---------|----------|-----------------|--------------|------------|
| Identity | 90% | 85% | 100% | 50+ |
| Patient | 85% | 75% | 95% | 40+ |
| Clinical | 85% | 70% | 95% | 40+ |
| Appointment | 80% | 70% | 85% | 35+ |
| Billing | 85% | 75% | 80% | 40+ |
| Prescription | 85% | 72% | 90% | 38+ |
| Notification | 80% | 65% | 75% | 30+ |
| Audit | 90% | 85% | 100% | 45+ |
| Analytics | 80% | 70% | 80% | 35+ |
| OutboxProcessor | 90% | 85% | 80% | 45+ |
| ApiGateway | 85% | 75% | 95% | 45+ |
| **TOTAL** | **≥85%** | **≥70%** | **≥90%** | **≥450** |

## File Structure Template

For each service, create:

```
backend/tests/
├── EHRPlatform.Tests.Unit/
│   └── Application/
│       ├── {ServiceName}ValidatorTests.cs      (20 tests)
│       └── Services/
│           └── {ServiceName}ServiceTests.cs    (10 tests)
│
├── EHRPlatform.Tests.Integration/
│   └── {ServiceName}/
│       ├── {ServiceName}IntegrationTests.cs    (15 tests)
│       ├── {ServiceName}RepositoryTests.cs     (10 tests)
│       └── {ServiceName}WorkflowTests.cs       (10 tests)
│
└── EHRPlatform.Tests.Security/
    ├── Authentication/
    │   └── {ServiceName}AuthTests.cs           (8 tests)
    ├── DataProtection/
    │   └── {ServiceName}PhiTests.cs            (10 tests)
    └── Audit/
        └── {ServiceName}AuditTests.cs          (10 tests)
```

## Quick Reference: Test Count Checklist

### Unit Tests (Per Service)
- [ ] 20 Validator tests
- [ ] 10 Service tests
- [ ] 5 Mapper tests (if applicable)
- **Subtotal: 35 per service**

### Integration Tests (Per Service)
- [ ] 15 Repository/CRUD tests
- [ ] 10 Workflow tests
- [ ] 5 Caching tests (if applicable)
- **Subtotal: 30 per service**

### Security Tests (Per Service)
- [ ] 8 Authentication tests
- [ ] 10 PHI protection tests
- [ ] 10 Audit compliance tests
- [ ] 5 Authorization tests (if applicable)
- **Subtotal: 33 per service**

**Total per service: ~100 tests**

## Service-Specific Considerations

### Identity Service
- JWT generation and validation
- Password hashing (bcrypt, scrypt)
- Role-based access control (RBAC)
- Multi-factor authentication (MFA)
- Session management

### Patient Service
- Patient demographic validation
- MRN uniqueness
- Insurance information
- Emergency contacts
- Medical history

### Clinical Service
- Diagnosis codes (ICD-10)
- Clinical note encryption
- Provider credentials
- Specialty-specific workflows
- Drug interactions

### Appointment Service
- Scheduling conflicts
- Doctor availability
- Time zone handling
- Reminder logic
- Cancellation policies

### Billing Service
- Invoice generation
- Payment processing
- Insurance claims
- Tax calculations
- Refund processing

### Prescription Service
- Medication verification
- Dosage validation
- Drug interaction checking
- Pharmacy integration
- Refill logic

### Notification Service
- Email delivery
- SMS delivery
- Push notifications
- Retry logic
- Rate limiting

### Audit Service
- Immutable logs
- Access tracking
- Data modification trail
- Retention enforcement
- Export functionality

### Analytics Service
- Report generation
- Aggregation queries
- Time-series data
- Performance metrics
- Data consistency

### OutboxProcessor Service
- Event publishing
- Saga orchestration
- Dead-letter handling
- Retry policies
- Idempotency

### ApiGateway Service
- Route validation
- Rate limiting
- Request/response transformation
- JWT verification
- CORS policies

## Running Tests by Service

```bash
# Run all tests for specific service
dotnet test backend/EHRPlatform.sln --filter "FullyQualifiedName~ClinicalService"

# Run only unit tests for service
dotnet test backend/EHRPlatform.sln --filter "FullyQualifiedName~ClinicalService.Tests.Unit"

# Run only integration tests for service
dotnet test backend/EHRPlatform.sln --filter "FullyQualifiedName~ClinicalService.Tests.Integration"

# Run with coverage
dotnet test backend/EHRPlatform.sln /p:CollectCoverage=true /p:CoverageFormat=opencover
```

## CI/CD Integration

Add to GitHub Actions `.github/workflows/test.yml`:

```yaml
- name: Run Unit Tests
  run: dotnet test tests/EHRPlatform.Tests.Unit/ /p:CollectCoverage=true

- name: Run Integration Tests
  run: dotnet test tests/EHRPlatform.Tests.Integration/ /p:CollectCoverage=true

- name: Run Security Tests
  run: dotnet test tests/EHRPlatform.Tests.Security/ /p:CollectCoverage=true

- name: Check Coverage
  run: |
    if [ $COVERAGE -lt 85 ]; then
      echo "Coverage below 85%"
      exit 1
    fi
```

## Implementation Timeline

### Phase 1: Reference Implementation ✅
- PatientService (40 tests)
- AppointmentService (35 tests)
- BillingService (35 tests)
- AuditService (45 tests)
- **Total: ~155 tests**

### Phase 2: Core Services (Next)
- Identity Service (50 tests)
- Clinical Service (40 tests)
- Prescription Service (38 tests)
- **Total: ~128 tests**

### Phase 3: Supporting Services
- Notification Service (30 tests)
- Analytics Service (35 tests)
- OutboxProcessor Service (45 tests)
- ApiGateway Service (45 tests)
- **Total: ~155 tests**

## Success Criteria

- ✅ ≥450 total tests across all services
- ✅ ≥85% unit test coverage
- ✅ ≥70% integration test coverage
- ✅ ≥90% security test coverage for PHI/HIPAA paths
- ✅ All tests pass in CI/CD
- ✅ Average test execution < 2 minutes
- ✅ 100% of critical paths tested

## Common Pitfalls & Solutions

### Pitfall: Slow Database Tests
**Solution**: Use Testcontainers with proper connection pooling. Reset database between tests via transactions.

### Pitfall: Flaky Tests
**Solution**: Use explicit waits, proper transaction isolation, avoid hardcoded delays.

### Pitfall: Missing HIPAA Tests
**Solution**: Use HipaaComplianceHelper for every PHI-handling service.

### Pitfall: Test Data Contamination
**Solution**: Use builders (PatientBuilder) and IntegrationTestBase for isolation.

## Resources

- Reference Test Files:
  - `backend/tests/EHRPlatform.Tests.Unit/Domain/PatientTests.cs`
  - `backend/tests/EHRPlatform.Tests.Unit/Application/PatientValidatorTests.cs`
  - `backend/tests/EHRPlatform.Tests.Unit/Services/PatientServiceTests.cs`
  - `backend/tests/EHRPlatform.Tests.Integration/PatientService/`
  - `backend/tests/EHRPlatform.Tests.Security/DataProtection/`

- Test Infrastructure:
  - `backend/tests/EHRPlatform.Tests.Common/Builders/` (Builders)
  - `backend/tests/EHRPlatform.Tests.Common/Helpers/` (Generators, mocks)
  - `backend/tests/EHRPlatform.Tests.Common/Base/` (Base classes)
  - `backend/tests/EHRPlatform.Tests.Common/Fixtures/` (Containers)

---

**Last Updated**: 2026-07-28  
**Status**: Reference implementations complete, ready for expansion  
**Target Completion**: Phase 3 by end of sprint
