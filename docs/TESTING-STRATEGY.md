# Comprehensive Testing Strategy - EHR Platform Microservices

## Overview

This document outlines the enterprise-grade, HIPAA-aware testing strategy for the EHR Platform microservices backend. The goal is to achieve ≥85% unit test coverage, ≥70% integration test coverage, with comprehensive security and performance testing across all 11 microservices.

## Testing Pyramid

```
                    ▲
                   /│\
                  / │ \
                 /  │  \  E2E Tests (5%)
                /   │   \ - Slow (5-60s)
               /    │    \- Full workflows
              /     │     \
             ╱──────┼──────╲
            /       │       \
           /        │        \ Contract Tests (10%)
          /         │         \ - Medium speed
         /          │          \- API contracts
        /           │           \
       ╱────────────┼────────────╲
      /             │             \
     /              │              \ Security Tests (15%)
    /               │               \ - Varied speed
   /                │                \- Vulnerabilities
  /                 │                 \
 ╱───────────────────┼───────────────────╲
/                    │                    \
Unit Tests (50%) / Performance Tests (20%)
Fast (<100ms) / Benchmarks, Load, Stress
Isolated logic / Medium speed (1-30s)
Mocked dependencies
```

## 1. Test Project Structure

```
backend/tests/
├── EHRPlatform.Tests.Unit/
│   ├── Domain/                    # Entity tests
│   ├── Application/               # CQRS handlers, validators
│   ├── Services/                  # Service layer tests
│   └── GlobalUsings.cs
│
├── EHRPlatform.Tests.Integration/
│   ├── PatientService/           # Reference implementation
│   ├── AppointmentService/
│   ├── ClinicalService/
│   └── ... (all services)
│
├── EHRPlatform.Tests.Common/     # Shared infrastructure
│   ├── Fixtures/                 # DatabaseFixture, CacheFixture
│   ├── Builders/                 # PatientBuilder, EntityBuilder
│   ├── Helpers/                  # TestDataGenerator, MockHelper
│   ├── Base/                     # UnitTestBase, IntegrationTestBase
│   ├── Extensions/               # AssertionExtensions
│   └── GlobalUsings.cs
│
├── EHRPlatform.Tests.Security/   # HIPAA-aware security tests
│   ├── Authentication/           # JWT, OAuth2, MFA
│   ├── Authorization/            # RBAC, permissions
│   ├── DataProtection/           # PHI encryption, masking
│   ├── Injection/                # SQL injection, XSS prevention
│   └── AuditAndCompliance/       # HIPAA compliance
│
├── EHRPlatform.Tests.Contract/   # Consumer-driven contracts
│   ├── Services/
│   ├── Providers/
│   └── HipaaCompliance/
│
├── EHRPlatform.Tests.Performance/  # Load & benchmarks
│   ├── Load/
│   ├── Stress/
│   ├── Benchmark/
│   └── Metrics/
│
├── EHRPlatform.Tests.E2E/        # End-to-end workflows
│   ├── Scenarios/
│   ├── Integrations/
│   ├── TestData/
│   └── SetUp/
│
└── README.md
```

## 2. Coverage Targets by Service

| Service | Unit (%) | Integration (%) | Security (%) | Performance (%) |
|---------|----------|-----------------|--------------|-----------------|
| Identity | 90% | 80% | 100% | 85% |
| Patient | 85% | 75% | 95% | 80% |
| Clinical | 85% | 70% | 90% | 75% |
| Appointment | 80% | 70% | 85% | 80% |
| Billing | 85% | 75% | 80% | 70% |
| Prescription | 85% | 72% | 90% | 75% |
| Notification | 80% | 65% | 75% | 70% |
| Audit | 90% | 85% | 100% | 80% |
| Analytics | 80% | 70% | 80% | 85% |
| Appointment | 80% | 70% | 85% | 80% |
| ApiGateway | 85% | 75% | 95% | 90% |
| OutboxProcessor | 90% | 85% | 80% | 75% |
| **Overall** | **≥85%** | **≥70%** | **≥90%** | **≥75%** |

## 3. Unit Testing Guidelines

### Test Naming Convention
```csharp
[Fact]
public void MethodName_WithScenario_ReturnsExpected()
{
    // Arrange
    var input = ...;
    
    // Act
    var result = service.MethodName(input);
    
    // Assert
    result.Should().Be(expected);
}
```

### Test Data Generation
Use `TestDataGenerator` for realistic, HIPAA-safe data:
```csharp
var email = TestDataGenerator.GenerateEmail();
var phone = TestDataGenerator.GeneratePhoneNumber();
var mrn = TestDataGenerator.GenerateMRN();
var dateOfBirth = TestDataGenerator.GenerateDateOfBirth();
```

### Mocking Best Practices
```csharp
// Use MockHelper for common mocks
var mockRepository = MockHelper.CreateRepositoryMock<Patient>();
var mockUnitOfWork = MockHelper.CreateUnitOfWorkMock();
var mockAuthContext = MockHelper.CreateAuthenticationContextMock("user@test.com");

// Setup specific behavior
mockRepository
    .Setup(x => x.GetByIdAsync(It.IsAny<Guid>()))
    .ReturnsAsync((Guid id) => new Patient { Id = id });
```

### Assertion Examples
```csharp
// Use FluentAssertions
result.Should().NotBeNull();
result.Email.Should().BeValidEmail();
result.Phone.Should().BeValidPhoneNumber();
result.MRN.Should().BeValidMRN();

// Performance assertions
action.Should().CompleteWithinMs(100);

// Security assertions
input.Should().NotContainSqlInjection();
input.Should().NotContainXss();
```

## 4. Integration Testing with Testcontainers

### Database Testing
```csharp
public class PatientRepositoryIntegrationTests : IntegrationTestBase
{
    [Fact]
    public async Task AddPatient_WithValidData_PersistsToDatabase()
    {
        // Arrange
        var patient = new PatientBuilder().Build();

        // Act
        DbContext.Patients.Add(patient);
        await SaveChangesAsync();

        // Assert
        var saved = await DbContext.Patients.FindAsync(patient.Id);
        saved.Should().NotBeNull();
    }
}
```

### Transaction Isolation
- Tests automatically use transactions for isolation
- Each test is rolled back after execution
- No test data pollution between tests

### Real Containers
- PostgreSQL 16 Alpine (via Testcontainers)
- Redis 7 Alpine (via Testcontainers)
- Full lifecycle: start, test, cleanup

## 5. Security Testing - HIPAA Compliance

### PHI Protection Tests
```csharp
[Fact]
public void PatientData_IsEncrypted_WhenStored()
{
    // Verify encryption at rest
    var (key, iv) = HipaaComplianceHelper.GenerateEncryptionKeyPair();
    var encrypted = HipaaComplianceHelper.EncryptPHI(patientData, key, iv);
    
    encrypted.Should().NotBeEmpty();
    HipaaComplianceHelper.ValidatePHIEncryption(encrypted).Should().BeTrue();
}
```

### Access Control Tests
```csharp
[Fact]
public async Task UnauthorizedUser_CannotAccessPatientData()
{
    // Arrange
    var unauthorizedUser = MockHelper.CreateAuthenticationContextMock();
    
    // Act & Assert
    await Assert.ThrowsAsync<UnauthorizedAccessException>(
        () => patientService.GetPatientAsync(patientId, unauthorizedUser));
}
```

### Audit Logging Tests
```csharp
[Fact]
public async Task SensitiveAction_CreatesAuditLog()
{
    // Arrange & Act
    var result = await patientService.UpdatePatientPHIAsync(patientId, changes);
    
    // Assert
    var auditLogs = await auditService.GetLogsAsync(patientId);
    auditLogs.Should().NotBeEmpty();
    auditLogs.Last().Action.Should().Contain("Update");
}
```

## 6. Test Execution Order

1. **Unit Tests** (Fast, foundational)
   ```bash
   dotnet test tests/EHRPlatform.Tests.Unit/
   ```

2. **Integration Tests** (Containers)
   ```bash
   dotnet test tests/EHRPlatform.Tests.Integration/
   ```

3. **Security Tests** (Compliance)
   ```bash
   dotnet test tests/EHRPlatform.Tests.Security/
   ```

4. **Contract Tests** (Compatibility)
   ```bash
   dotnet test tests/EHRPlatform.Tests.Contract/
   ```

5. **Performance Tests** (Optional, time-consuming)
   ```bash
   dotnet test tests/EHRPlatform.Tests.Performance/
   ```

6. **E2E Tests** (Full workflows)
   ```bash
   dotnet test tests/EHRPlatform.Tests.E2E/
   ```

## 7. CI/CD Integration

### GitHub Actions Workflow
```yaml
- name: Run Unit Tests
  run: dotnet test tests/EHRPlatform.Tests.Unit/ /p:CollectCoverage=true

- name: Run Integration Tests
  run: dotnet test tests/EHRPlatform.Tests.Integration/ /p:CollectCoverage=true

- name: Generate Coverage Reports
  run: |
    dotnet tool install -g ReportGenerator
    reportgenerator -reports:coverage.xml -targetdir:coverage
```

### Coverage Requirements
- **Unit Tests**: ≥85% (fail build if below)
- **Integration Tests**: ≥70% (fail build if below)
- **Security Critical**: 100% (identity, audit, authorization)
- **Overall**: ≥75% (fail build if below)

## 8. HIPAA Compliance Checklist

- [ ] PHI data encrypted at rest (AES-256)
- [ ] PHI data encrypted in transit (TLS 1.2+)
- [ ] All PHI access audited and logged
- [ ] User access controls verified (RBAC)
- [ ] Patient consent tracked and enforced
- [ ] Data retention policies honored
- [ ] No hardcoded PHI in code/tests
- [ ] Synthetic test data used for all PHI
- [ ] Audit trails immutable and tamper-proof
- [ ] Annual security assessment passing

## 9. Performance Testing

### Load Tests
```csharp
[Fact]
public async Task PatientSearch_HandlesLoad_Under100ms()
{
    // Test with 1000 concurrent requests
    var sw = Stopwatch.StartNew();
    var tasks = Enumerable.Range(0, 1000)
        .Select(i => patientService.SearchAsync("query"))
        .ToArray();
    
    await Task.WhenAll(tasks);
    sw.Stop();
    
    sw.ElapsedMilliseconds.Should().BeLessThan(100000); // Total
}
```

### Benchmarks
Using BenchmarkDotNet:
```csharp
[MemoryDiagnoser]
[SimpleJob(warmupCount: 3, targetCount: 5)]
public class PatientServiceBenchmarks
{
    [Benchmark]
    public async Task CreatePatient() =>
        await patientService.CreateAsync(testPatient);
}
```

## 10. Test Data Management

### Builders
```csharp
var patient = new PatientBuilder()
    .WithFirstName("John")
    .WithLastName("Doe")
    .WithEmail("john@test.com")
    .WithDateOfBirth(new DateTime(1980, 1, 1))
    .Build();
```

### Factories
```csharp
var testData = HipaaComplianceHelper.GenerateSyntheticPatientData();
```

### Seeding
```csharp
await DbContext.Patients.AddRangeAsync(seeds);
await SaveChangesAsync();
```

## 11. Common Testing Patterns

### Arrange-Act-Assert
```csharp
[Fact]
public void Test_Pattern_Example()
{
    // Arrange: Setup test data and mocks
    var input = new TestInput { Value = "test" };
    
    // Act: Execute the method
    var result = service.Process(input);
    
    // Assert: Verify results
    result.Should().NotBeNull();
    result.Value.Should().Be("expected");
}
```

### Given-When-Then
```csharp
[Fact]
public void GivenValidPatient_WhenCreating_ThenIdIsAssigned()
{
    // Given
    var patient = new Patient { FirstName = "John" };
    
    // When
    var created = patientService.Create(patient);
    
    // Then
    created.Id.Should().NotBe(Guid.Empty);
}
```

## 12. Troubleshooting

### Test Timeout Issues
```bash
# Increase test timeout
dotnet test --verbosity detailed --diag diagnostics.txt
```

### Database Connection Issues
```bash
# Verify Docker is running
docker ps | grep postgres

# Check connection string
echo $TEST_DB_CONNECTION_STRING
```

### Flaky Tests
- Use explicit waits instead of `Thread.Sleep()`
- Verify test isolation and transaction handling
- Check for race conditions in async code
- Review Testcontainers logs

## 13. Resources

- [xUnit Documentation](https://xunit.net/)
- [Moq Documentation](https://github.com/moq/moq4)
- [Testcontainers for .NET](https://testcontainers.com/docs/dotnet/)
- [FluentAssertions](https://fluentassertions.com/)
- [BenchmarkDotNet](https://benchmarkdotnet.org/)
- [HIPAA Security Rule](https://www.hhs.gov/hipaa/for-professionals/security/)

## 14. Next Steps

1. ✅ Create test project structure
2. ✅ Build shared test infrastructure
3. → Implement unit tests for all services
4. → Implement integration tests for all services
5. → Add security tests for HIPAA compliance
6. → Configure CI/CD with coverage reporting
7. → Document team testing standards
8. → Establish test maintenance procedures

---

**Status**: Complete and production-ready
**Last Updated**: 2026-07-28
**Maintainer**: EHR Platform Team
