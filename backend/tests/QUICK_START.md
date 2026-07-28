# Quick Start - Testing

## Run Tests Immediately

```bash
# All tests
dotnet test backend/EHRPlatform.sln

# Specific test project
dotnet test tests/EHRPlatform.Tests.Unit/EHRPlatform.Tests.Unit.csproj

# With coverage report
dotnet test backend/EHRPlatform.sln /p:CollectCoverage=true /p:CoverageFormat=opencover
```

## Test Projects Overview

| Project | Purpose | Speed | Notes |
|---------|---------|-------|-------|
| **Unit** | Logic isolation | ⚡ Fast | Unit & fast integration |
| **Common** | Shared infrastructure | N/A | Base classes & utilities |
| **Contract** | API contracts | 🟡 Medium | Pact-based validation |
| **Security** | Auth & compliance | 🟡 Medium | HIPAA, injection, access control |
| **Performance** | Load & benchmarks | 🔴 Slow | Load testing, stress tests |
| **E2E** | Full workflows | 🔴 Slow | Complete business scenarios |

## Quick Setup

### Prerequisites
```bash
# Install .NET 8.0 SDK
# Docker for Testcontainers

# Verify setup
dotnet --version
docker --version
```

### First Run
```bash
cd backend
dotnet restore
dotnet test tests/EHRPlatform.Tests.Unit/EHRPlatform.Tests.Unit.csproj
```

## Writing Your First Test

### Unit Test Template
```csharp
using Xunit;
using EHRPlatform.Tests.Common.Builders;

namespace EHRPlatform.Tests.Unit.Services;

public class MyServiceTests
{
    private readonly MyService _service = new();

    [Fact]
    public void DoSomething_WithValidInput_ReturnsExpected()
    {
        // Arrange
        var input = "test";

        // Act
        var result = _service.DoSomething(input);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("expected", result);
    }
}
```

### Run Your Test
```bash
dotnet test tests/EHRPlatform.Tests.Unit/ --filter "MyServiceTests"
```

## Test Utilities

### Generate Test Data
```csharp
using EHRPlatform.Tests.Common.Helpers;

var id = TestDataGenerator.GenerateId();
var email = TestDataGenerator.GenerateEmail();
var date = TestDataGenerator.GenerateRandomDate();
```

### Create Mock Objects
```csharp
using EHRPlatform.Tests.Common.Helpers;
using Moq;

var mock = MockHelper.CreateStrictMock<IMyInterface>();
mock.Setup(x => x.Method()).Returns("value");
```

### Build Complex Objects
```csharp
using EHRPlatform.Tests.Common.Builders;

var patient = new TestEntityBuilder<Patient>()
    .WithName("John Doe")
    .WithEmail("john@test.com")
    .Build();
```

## Common Commands

```bash
# Run specific test class
dotnet test backend/EHRPlatform.sln --filter "FullyQualifiedName~PatientServiceTests"

# Run with pattern matching
dotnet test backend/EHRPlatform.sln --filter "Category=Security"

# Run in verbose mode
dotnet test backend/EHRPlatform.sln --verbosity detailed

# Run tests in parallel
dotnet test backend/EHRPlatform.sln -p:ParallelizeTestCollections=true

# Stop on first failure
dotnet test backend/EHRPlatform.sln --blame

# See test names without running
dotnet test backend/EHRPlatform.sln --list-tests
```

## Test Configuration

Set environment variables for test configuration:

```bash
# API endpoint
export TEST_API_BASE_URL=http://localhost:5000

# Database
export TEST_DB_CONNECTION_STRING="Host=localhost;Database=ehr_test;..."

# Redis
export TEST_REDIS_CONNECTION_STRING="localhost:6379"

# Credentials
export TEST_ADMIN_EMAIL=admin@test.local
export TEST_ADMIN_PASSWORD=AdminPass123!

# Enable HIPAA compliance mode
export TEST_HIPAA_MODE=true
```

## Debugging

### Debug in Visual Studio
1. Set breakpoint in test
2. Right-click test → Debug Tests
3. Use debugger normally

### Debug from Command Line
```bash
# Enable debug mode
dotnet test -c Debug --logger "console;verbosity=detailed" --filter "TestName"
```

### View Test Output
```bash
# Verbose output
dotnet test backend/EHRPlatform.sln -v detailed

# With diagnostics
dotnet test backend/EHRPlatform.sln --diag diagnostics.txt
```

## Troubleshooting

### Tests Won't Run
```bash
# Clean build
dotnet clean
dotnet build

# Restore packages
dotnet restore
```

### Connection Issues
```bash
# Check database
docker ps | grep postgres

# Check Redis
docker ps | grep redis

# Start Docker containers
docker-compose -f docker-compose.yml up -d
```

### Slow Tests
```bash
# Run only fast tests
dotnet test tests/EHRPlatform.Tests.Unit/

# Skip performance tests
dotnet test backend/EHRPlatform.sln --filter "FullyQualifiedName!~Performance"
```

## Project Structure

```
tests/
├── README.md                      ← Full documentation
├── TESTING_GUIDE.md               ← Comprehensive guide
├── QUICK_START.md                 ← This file
├── STRUCTURE.md                   ← Architecture details
├── TestConfiguration.cs           ← Central config
│
├── EHRPlatform.Tests.Unit/        ← Fast unit tests
├── EHRPlatform.Tests.Common/      ← Shared utilities
├── EHRPlatform.Tests.Security/    ← Security & compliance
├── EHRPlatform.Tests.Contract/    ← API contracts
├── EHRPlatform.Tests.E2E/         ← Full workflows
└── EHRPlatform.Tests.Performance/ ← Load & benchmarks
```

## Next Steps

1. ✅ Review this QUICK_START.md
2. → Read specific project README.md for details
3. → Review TESTING_GUIDE.md for best practices
4. → Start writing tests following templates
5. → Run tests frequently during development

## Resources

- **xUnit**: https://xunit.net/docs/getting-started/
- **Moq**: https://github.com/moq/moq4/wiki
- **Testcontainers**: https://testcontainers.com/docs/dotnet/
- **FluentAssertions**: https://fluentassertions.com/

## Support

For testing questions:
1. Check project README.md files
2. Review TESTING_GUIDE.md
3. Check existing test examples
4. Ask in team chat or create an issue

---

**Happy Testing!** 🧪
