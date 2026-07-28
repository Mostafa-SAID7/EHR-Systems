# Comprehensive Testing Guide

Complete guide for testing the EHR Platform microservices backend.

## Quick Start

```bash
# Run all tests
dotnet test backend/EHRPlatform.sln

# Run specific test project
dotnet test tests/EHRPlatform.Tests.Unit/EHRPlatform.Tests.Unit.csproj

# Run with coverage
dotnet test backend/EHRPlatform.sln /p:CollectCoverage=true /p:CoverageFormat=opencover
```

## Test Structure Overview

```
tests/
├── EHRPlatform.Tests.Unit/           # Fast unit tests
├── EHRPlatform.Tests.Common/         # Shared test infrastructure
├── EHRPlatform.Tests.Contract/       # Consumer-driven contracts
├── EHRPlatform.Tests.Security/       # Security & compliance
├── EHRPlatform.Tests.Performance/    # Load & benchmarks
├── EHRPlatform.Tests.E2E/            # End-to-end workflows
└── README.md                          # Main testing documentation
```

## Test Execution Pyramid

```
                    ▲
                   /│\
                  / │ \
                 /  │  \  E2E Tests (5%)
                /   │   \ - Slow (5-60s)
               /    │    \ - Full workflows
              /     │     \
             ╱──────┼──────╲
            /       │       \
           /        │        \ Contract Tests (10%)
          /         │         \ - Medium speed
         /          │          \ - API contracts
        /           │           \
       ╱────────────┼────────────╲
      /             │             \
     /              │              \ Security Tests (15%)
    /               │               \ - Varied speed
   /                │                \ - Vulnerabilities
  /                 │                 \
 ╱───────────────────┼───────────────────╲
/                    │                    \
Performance Tests (20%) / Unit Tests (50%)
Load, stress, benchmarks - Medium speed / Fast (<100ms)
- API endpoints         - Isolated logic
- Query performance    - Mocked dependencies
```

## Test Types and Execution Times

| Type | Count | Speed | Purpose |
|------|-------|-------|---------|
| Unit | ~500 | <100ms total | Logic validation |
| Contract | ~100 | 1-5s | API compatibility |
| Security | ~150 | 2-10s | Security compliance |
| Performance | ~50 | 5-30s | Load & benchmarks |
| E2E | ~30 | 5-60s | Workflow validation |
| **Total** | **~830** | **~60-90s** | Complete coverage |

## Test Execution Order

1. **Unit Tests** (baseline, fastest)
2. **Contract Tests** (service dependencies)
3. **Security Tests** (compliance validation)
4. **Performance Tests** (optional, time-consuming)
5. **E2E Tests** (final validation)

## Writing Tests: Templates

### Unit Test Template
```csharp
[Fact]
public void MethodName_WithScenario_ReturnsExpectedResult()
{
    // Arrange
    var service = new MyService();
    var input = new TestData();

    // Act
    var result = service.Method(input);

    // Assert
    result.ShouldNotBeNull();
    Assert.Equal(expected, result.Value);
}
```

### E2E Test Template
```csharp
public class WorkflowTests : E2ETestBase
{
    [Fact]
    public async Task CompleteWorkflow_WithValidData_Succeeds()
    {
        // Arrange
        var request = new WorkflowRequest { ... };

        // Act
        var response = await PostAsync<WorkflowResponse>("/api/v1/workflows", request);

        // Assert
        response.ShouldNotBeNull();
        Assert.Equal("Completed", response.Status);
    }
}
```

### Contract Test Template
```csharp
public class ServiceContractTests : ContractTestBase
{
    [Fact]
    public async Task ServiceApi_ReturnsCorrectContract()
    {
        // Arrange & Act
        var response = await GetFromService<ServiceResponse>();

        // Assert
        ValidateContractCompliance(response, typeof(ServiceResponse));
    }
}
```

## Best Practices

### 1. Test Naming
```csharp
// ✓ GOOD: Clear intent
[Fact]
public void CreatePatient_WithValidData_ReturnsPatientWithId()

// ✗ BAD: Vague
[Fact]
public void TestPatient()
```

### 2. Arrange-Act-Assert Pattern
```csharp
// ✓ GOOD: Clear sections
[Fact]
public void Test()
{
    // Arrange
    var patient = CreateTestPatient();

    // Act
    var result = _service.SavePatient(patient);

    // Assert
    Assert.NotNull(result);
}
```

### 3. Test Isolation
```csharp
// ✓ GOOD: Independent tests
[Fact]
public void Test1() { }

[Fact]
public void Test2() { } // Doesn't depend on Test1

// ✗ BAD: Dependent tests
[Fact]
public void Test1() { /* creates data */ }

[Fact]
public void Test2() { /* uses data from Test1 */ }
```

### 4. Use Builders for Complex Objects
```csharp
// ✓ GOOD: Readable and maintainable
var patient = new PatientBuilder()
    .WithName("John Doe")
    .WithDateOfBirth(DateTime.Parse("1980-01-01"))
    .Build();

// ✗ BAD: Hard to read
var patient = new Patient 
{ 
    FirstName = "John",
    LastName = "Doe",
    MiddleName = "",
    DateOfBirth = DateTime.Parse("1980-01-01"),
    Gender = "M",
    // ... 20 more properties
};
```

### 5. Assert Meaningful Conditions
```csharp
// ✓ GOOD: Validates actual behavior
Assert.True(patient.IsActive);
Assert.Contains("error", response);

// ✗ BAD: Meaningless assertions
Assert.NotNull(result);
Assert.True(true);
```

## Running Tests Locally

### Run All Tests
```bash
dotnet test backend/EHRPlatform.sln
```

### Run Specific Project
```bash
dotnet test tests/EHRPlatform.Tests.Unit/EHRPlatform.Tests.Unit.csproj
```

### Run Specific Test Class
```bash
dotnet test backend/EHRPlatform.sln --filter "FullyQualifiedName~PatientServiceTests"
```

### Run Specific Test Method
```bash
dotnet test backend/EHRPlatform.sln --filter "FullyQualifiedName~PatientServiceTests.Create_WithValidData_ReturnsPatient"
```

### Run with Coverage
```bash
dotnet test backend/EHRPlatform.sln /p:CollectCoverage=true /p:CoverageFormat=opencover /p:Exclude="[*]EHRPlatform.Common.Migrations.*"
```

### Run with Detailed Output
```bash
dotnet test backend/EHRPlatform.sln --verbosity detailed --logger "console;verbosity=detailed"
```

### Run Tests in Parallel
```bash
dotnet test backend/EHRPlatform.sln --parallel-workers=4
```

## Debugging Tests

### Debug Test in Visual Studio
1. Set breakpoint in test
2. Right-click test → Debug Tests
3. Debugger will break at breakpoint

### Debug Test from Command Line
```bash
dotnet test tests/EHRPlatform.Tests.Unit/EHRPlatform.Tests.Unit.csproj --filter "TestName" --diag
