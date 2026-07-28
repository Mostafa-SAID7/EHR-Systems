# EHR Platform - Comprehensive Test Suite

This directory contains all test projects for the EHR Platform microservices backend.

## Test Projects

### 1. **EHRPlatform.Tests.Unit**
Located in `EHRPlatform.Tests.Unit/`

Unit tests for individual components and business logic.

**Key Features:**
- Service layer tests
- Query/Command handler tests
- Entity validation tests
- Mapper tests
- Repository tests

**Run Command:**
```bash
dotnet test tests/EHRPlatform.Tests.Unit/EHRPlatform.Tests.Unit.csproj
```

---

### 2. **EHRPlatform.Tests.Common**
Located in `EHRPlatform.Tests.Common/`

Shared testing infrastructure, fixtures, builders, and utilities used across all test projects.

**Contents:**
- **Fixtures/**: Database, Cache, MessageQueue, and HttpClient test fixtures
- **Builders/**: Entity builders, Query builders, and TestEntityBuilder base classes
- **Helpers/**: TestDataGenerator, MockHelper, and assertion utilities
- **Extensions/**: AssertionExtensions for common test operations

**Run Command:**
```bash
dotnet test tests/EHRPlatform.Tests.Common/EHRPlatform.Tests.Common.csproj
```

---

### 3. **EHRPlatform.Tests.Contract**
Located in `EHRPlatform.Tests.Contract/`

Contract testing for microservice interfaces and third-party integrations using Pact and Testcontainers.

**Key Features:**
- Consumer-driven contract tests
- Service provider validation
- API contract compliance verification
- HIPAA compliance validation
- Third-party provider contract tests

**Subdirectories:**
- `Services/`: Inter-service contracts (Identity, Patient, Clinical, etc.)
- `Providers/`: External provider contracts (Payment, Insurance, etc.)
- `Fixtures/`: Contract test fixtures
- `HipaaCompliance/`: HIPAA-specific contract validations

**Run Command:**
```bash
dotnet test tests/EHRPlatform.Tests.Contract/EHRPlatform.Tests.Contract.csproj
```

---

### 4. **EHRPlatform.Tests.Security**
Located in `EHRPlatform.Tests.Security/`

Security testing including authentication, authorization, data protection, and vulnerability assessments.

**Key Features:**
- Authentication and JWT validation
- Authorization and RBAC testing
- Data protection and encryption verification
- Injection attack prevention tests
- Audit and compliance validation
- HIPAA security requirements

**Subdirectories:**
- `Authentication/`: JWT, OAuth2, MFA, session tests
- `Authorization/`: RBAC, ABAC, permission tests
- `DataProtection/`: Encryption, PII/PHI protection
- `Injection/`: SQL injection, XSS, command injection prevention
- `Vulnerabilities/`: Common vulnerability assessments
- `AuditAndCompliance/`: Audit trail and compliance tests

**Run Command:**
```bash
dotnet test tests/EHRPlatform.Tests.Security/EHRPlatform.Tests.Security.csproj
```

---

### 5. **EHRPlatform.Tests.Performance**
Located in `EHRPlatform.Tests.Performance/`

Performance and load testing using BenchmarkDotNet and NBomber.

**Key Features:**
- Load testing with concurrent users
- Stress testing and system limits
- Performance benchmarking
- Resource utilization tracking
- Bottleneck identification
- Latency analysis

**Subdirectories:**
- `Load/`: Concurrent user simulation and throughput tests
- `Stress/`: System limits and extreme load tests
- `Benchmark/`: Performance benchmarks for critical paths
- `Metrics/`: Performance metrics collection and reporting

**Run Command:**
```bash
dotnet test tests/EHRPlatform.Tests.Performance/EHRPlatform.Tests.Performance.csproj
```

---

### 6. **EHRPlatform.Tests.E2E**
Located in `EHRPlatform.Tests.E2E/`

End-to-end tests covering complete business workflows across multiple services.

**Key Features:**
- Full business workflow testing
- Cross-service integration validation
- Database state verification
- Event propagation testing
- Testcontainer support for full environment setup

**Subdirectories:**
- `Scenarios/`: Business workflow scenarios
- `Integrations/`: Cross-service integration tests
- `TestData/`: E2E test data and factories
- `SetUp/`: Environment and fixture setup
- `TearDown/`: Environment cleanup and verification

**Run Command:**
```bash
dotnet test tests/EHRPlatform.Tests.E2E/EHRPlatform.Tests.E2E.csproj
```

---

## Running All Tests

```bash
# Run all tests
dotnet test backend/EHRPlatform.sln

# Run with specific verbosity
dotnet test backend/EHRPlatform.sln --verbosity normal

# Run with coverage
dotnet test backend/EHRPlatform.sln /p:CollectCoverage=true

# Run specific test class
dotnet test backend/EHRPlatform.sln --filter "FullyQualifiedName~EHRPlatform.Tests.Unit.Services"
```

---

## Test Project Dependencies

```
EHRPlatform.Tests.E2E
├── EHRPlatform.Tests.Common
└── All Service Projects

EHRPlatform.Tests.Contract
├── EHRPlatform.Tests.Common
└── All Service Projects

EHRPlatform.Tests.Security
├── EHRPlatform.Tests.Common
├── EHRPlatform.Services.Identity
└── EHRPlatform.Services.Audit

EHRPlatform.Tests.Performance
├── EHRPlatform.Tests.Common
└── All Service Projects

EHRPlatform.Tests.Common
└── EHRPlatform.Common
```

---

## Test Infrastructure

### Shared Test Infrastructure (EHRPlatform.Tests.Common)

**Fixtures:**
- `DatabaseFixture`: PostgreSQL Testcontainer support
- `CacheFixture`: Redis Testcontainer support
- `MessageQueueFixture`: RabbitMQ/Kafka Testcontainer support
- `HttpClientFixture`: HTTP client with proper timeout handling

**Test Data Helpers:**
- `TestDataGenerator`: Random data generation (IDs, emails, dates, etc.)
- `MockHelper`: Mock creation and management

**Base Classes:**
- `SecurityTestBase`: Common security test setup
- `E2ETestBase`: E2E test HTTP client and utilities
- `PerformanceTestBase`: Performance measurement utilities
- `ContractTestBase`: Contract compliance validation

---

## Writing New Tests

### Unit Test Template
```csharp
using Xunit;
using EHRPlatform.Tests.Common.Builders;

namespace EHRPlatform.Tests.Unit.Services;

public class MyServiceTests
{
    [Fact]
    public void Method_WithValidInput_ReturnsExpectedResult()
    {
        // Arrange
        var service = new MyService();
        
        // Act
        var result = service.Method(validInput);
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal(expected, result.Value);
    }
}
```

### E2E Test Template
```csharp
using Xunit;
using EHRPlatform.Tests.E2E;

namespace EHRPlatform.Tests.E2E.Scenarios;

public class PatientOnboardingScenarioTests : E2ETestBase
{
    [Fact]
    public async Task PatientRegistration_WithValidData_CompletesSuccessfully()
    {
        // Arrange
        var patientData = new { Name = "John Doe", Email = "john@test.com" };
        
        // Act
        var response = await PostAsync<PatientDto>("api/v1/patients", patientData);
        
        // Assert
        Assert.NotNull(response);
        Assert.NotEmpty(response.Id);
    }
}
```

### Contract Test Template
```csharp
using Xunit;
using EHRPlatform.Tests.Contract;

namespace EHRPlatform.Tests.Contract.Services;

public class PatientServiceContractTests : ContractTestBase
{
    [Fact]
    public void CreatePatient_WithValidRequest_ReturnsExpectedContract()
    {
        // Arrange
        var request = new { Name = "John Doe" };
        
        // Act
        ValidateContractCompliance(request, typeof(PatientResponse));
        
        // Assert
        Assert.True(true); // Contract is valid
    }
}
```

### Security Test Template
```csharp
using Xunit;
using EHRPlatform.Tests.Security;

namespace EHRPlatform.Tests.Security.Authentication;

public class AuthenticationTests : SecurityTestBase
{
    [Fact]
    public void ValidToken_IsAccepted()
    {
        // Arrange
        var token = ValidToken;
        
        // Act & Assert
        Assert.NotEmpty(token);
    }

    [Fact]
    public void InvalidToken_IsRejected()
    {
        // Arrange
        var token = InvalidToken;
        
        // Act & Assert
        Assert.NotEmpty(token);
        // Actual validation would happen here
    }
}
```

---

## CI/CD Integration

Tests are automatically run on:
- Pull requests (via GitHub Actions)
- Commits to main branch
- Release builds

See `.github/workflows/test.yml` for CI configuration.

---

## Coverage Goals

- **Overall**: ≥80% code coverage
- **Security**: 100% coverage for authentication/authorization
- **Business Logic**: ≥85% coverage
- **Data Access**: ≥90% coverage

---

## Best Practices

1. **Test Isolation**: Each test should be independent and runnable in any order
2. **Clear Naming**: Use descriptive names following `Method_Scenario_ExpectedResult` pattern
3. **Arrangement**: Keep Arrange sections concise using builders and fixtures
4. **No Flakiness**: Avoid timing-dependent tests; use proper waits
5. **Performance**: Keep individual test execution time <100ms for unit tests
6. **Documentation**: Add XML comments to complex test scenarios
7. **Data Cleanup**: Always clean up test data after test execution
8. **HIPAA Compliance**: Never use real patient data; use synthetic test data

---

## Troubleshooting

### Database Connection Issues
- Ensure Docker is running for Testcontainers
- Check database connection strings in test configuration
- Verify PostgreSQL port is not in use

### Test Timeouts
- Increase timeout in test configuration
- Check for deadlocks in concurrent tests
- Review database query performance

### Flaky Tests
- Check for race conditions
- Verify proper test isolation
- Use explicit waits instead of Thread.Sleep

---

## Contributing

When adding new tests:
1. Create test in appropriate project
2. Follow naming conventions
3. Add XML documentation
4. Ensure test passes locally
5. Run full test suite before submitting PR

---

## Resources

- [xUnit Documentation](https://xunit.net/)
- [Moq Documentation](https://github.com/moq/moq4)
- [Testcontainers for .NET](https://testcontainers.com/docs/dotnet/)
- [BenchmarkDotNet](https://benchmarkdotnet.org/)
- [NBomber](https://nbomber.com/)
- [HIPAA Security Rule](https://www.hhs.gov/hipaa/for-professionals/security/index.html)
