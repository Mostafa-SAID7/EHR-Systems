# EHR Integration Tests

Comprehensive integration test suite for EHR platform tag management across Patient, Appointment, and Billing services.

## Project Structure

```
backend/tests/EHRPlatform.Tests.Integration/
├── IntegrationTestBase.cs          # Base class with IAsyncLifetime pattern
├── TestDbContext.cs                # In-memory SQLite database context
├── GlobalUsings.cs                 # Global using directives
├── Features/
│   └── Tags/
│       ├── PatientTagsIntegrationTests.cs         # 8 tests
│       ├── AppointmentTagsIntegrationTests.cs     # 8 tests
│       └── InvoiceTagsIntegrationTests.cs         # 8 tests
└── README.md                       # This file
```

## Test Infrastructure

### IntegrationTestBase

Base class for all integration tests providing:
- **IAsyncLifetime Pattern**: Automatic database setup/teardown
- **In-Memory SQLite Database**: More realistic than InMemoryDatabase provider
- **Dependency Injection**: Service collection with Mediator and mocks
- **Test Helpers**: Methods to create test tags and associations

```csharp
public abstract class IntegrationTestBase : IAsyncLifetime
{
    protected DbContextOptions<TestDbContext> DbContextOptions { get; }
    protected TestDbContext DbContext { get; }
    protected IMediator Mediator { get; }
    protected Mock<ITagService> MockTagService { get; }
    protected Mock<ITagQueryService> MockTagQueryService { get; }
    
    public virtual async Task InitializeAsync() { }
    public virtual async Task DisposeAsync() { }
    
    protected Tag CreateTag(string name, string category, ...);
    protected TagAssociation CreateTagAssociation(Guid tagId, Guid resourceId, ...);
}
```

### TestDbContext

Entity Framework Core context configured for testing:
- SQLite in-memory provider
- Tag entity with unique indexes on (Name, Category) and Slug
- TagAssociation entity with composite key and foreign key constraints
- Auditable entity support with timestamps

## Test Coverage

### PatientTagsIntegrationTests (8 tests)

1. **ApplyTag_SingleTag_CreatesAssociationSuccessfully** ✓
   - Happy path: Single tag application

2. **ApplyTags_MultipleTags_AppliesAllSuccessfully** ✓
   - Happy path: Multiple tags at once

3. **GetPatientTags_WithExistingTags_ReturnsAllTags** ✓
   - Query: Retrieve applied tags

4. **GetPatientTags_WithNoTags_ReturnsEmptyList** ✓
   - Query: Empty list handling

5. **RemovePatientTag_ValidTag_RemovesSuccessfully** ✓
   - Remove: Delete tag from patient

6. **ApplyTag_DuplicateTag_IsIdempotent** ✓
   - Edge case: Duplicate tag handling

7. **ApplyTag_NonExistentTag_ReturnsPartialSuccess** ✓
   - Edge case: Invalid tag ID graceful failure

8. **ApplyTags_ConcurrentRequests_HandlesConcurrencyCorrectly** ✓
   - Concurrency: Parallel tag operations

### AppointmentTagsIntegrationTests (8 tests)

1. **ApplyTag_SingleTag_SuccessfullyApplied** ✓
   - Happy path: Appointment status tags

2. **ApplyTags_MultipleAppointmentTags_AppliesSuccessfully** ✓
   - Happy path: Multiple format/priority tags

3. **GetAppointmentTags_WithTags_ReturnsAllTags** ✓
   - Query: Retrieve appointment tags

4. **GetAppointmentTags_NoTags_ReturnsEmpty** ✓
   - Query: Empty result handling

5. **RemoveAppointmentTag_ValidTag_RemovesSuccessfully** ✓
   - Remove: Delete appointment tag

6. **RemoveAppointmentTag_NonExistentTag_ReturnsNotFound** ✓
   - Edge case: Remove non-existent tag

7. **ApplyTag_ServiceRestricted_FailsForWrongService** ✓
   - Edge case: Service restriction enforcement

8. **ApplyTags_ConcurrentUpdates_HandlesConcurrency** ✓
   - Concurrency: Parallel appointment updates

### InvoiceTagsIntegrationTests (8 tests)

1. **ApplyTag_SingleBillingTag_SuccessfullyApplied** ✓
   - Happy path: Billing status tags

2. **ApplyTags_MultipleBillingTags_AppliesSuccessfully** ✓
   - Happy path: Payment method + compliance tags

3. **GetInvoiceTags_WithTags_ReturnsAllTags** ✓
   - Query: Retrieve invoice tags

4. **GetInvoiceTags_NoTags_ReturnsEmpty** ✓
   - Query: Empty result handling

5. **RemoveInvoiceTag_ValidTag_RemovesSuccessfully** ✓
   - Remove: Delete invoice tag

6. **ApplyTag_InvalidResourceType_HandlesGracefully** ✓
   - Edge case: Invalid resource type handling

7. **ApplyTag_ArchivedTag_CannotBeApplied** ✓
   - Edge case: Archived tag restrictions

8. **ApplyTag_TrackingUsageCount_IncrementsProperly** ✓
   - Tracking: Usage count verification

## Running Tests

### Build and Test
```bash
# Build integration tests
cd backend/tests/EHRPlatform.Tests.Integration
dotnet build

# Run all integration tests
dotnet test

# Run with detailed output
dotnet test -v normal

# Run specific test class
dotnet test --filter "PatientTagsIntegrationTests"

# Run with code coverage
dotnet test /p:CollectCoverage=true /p:CoverageFormat=opencover
```

### Project Dependencies

The integration test project references:
- **EHRPlatform.Common** - Tag entities, commands, interfaces
- **EHRPlatform.Services.Patient** - PatientTagsController
- **EHRPlatform.Services.Appointment** - AppointmentTagsController
- **EHRPlatform.Services.Billing** - InvoiceTagsController

### NuGet Dependencies

```xml
<PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.8.0" />
<PackageReference Include="Microsoft.Data.Sqlite" Version="8.0.0" />
<PackageReference Include="EntityFrameworkCore.InMemory" Version="8.0.0" />
<PackageReference Include="xunit" Version="2.6.2" />
<PackageReference Include="Moq" Version="4.20.70" />
<PackageReference Include="MediatR" Version="12.1.1" />
<PackageReference Include="Microsoft.Extensions.DependencyInjection" Version="8.0.0" />
```

## Test Patterns

### Arrange-Act-Assert with Mocks

```csharp
[Fact]
public async Task ApplyTag_SingleTag_SuccessfullyApplied()
{
    // Arrange: Setup test data
    var patientId = Guid.NewGuid();
    var tag = CreateTag("VIP", "Priority");
    await DbContext.Tags.AddAsync(tag);
    await DbContext.SaveChangesAsync();

    var command = new ApplyTagsCommand
    {
        ResourceId = patientId,
        ResourceType = nameof(PatientEntity),
        TagIds = new[] { tag.Id },
        ServiceName = "Patient"
    };

    // Mock service behavior
    MockTagService
        .Setup(x => x.GetByIdAsync(tag.Id, It.IsAny<CancellationToken>()))
        .ReturnsAsync(tag);

    // Act: Execute test
    var result = await _controller.ApplyPatientTags(
        patientId, command, CancellationToken.None);

    // Assert: Verify outcome
    var okResult = Assert.IsType<OkObjectResult>(result);
    var response = okResult.Value as TagAssignmentResponse;
    Assert.NotNull(response);
    Assert.True(response.Success);
    Assert.Single(response.AppliedTagIds);
}
```

### Database Isolation

Each test gets:
1. Fresh in-memory SQLite database
2. Automatic schema creation via `EnsureCreatedAsync()`
3. Complete cleanup via `EnsureDeletedAsync()`
4. No test data leakage between tests

### Mocking Strategy

- **ITagService**: Mocked for tag operations
- **ITagQueryService**: Mocked for queries
- **Mediator**: Real instance (tests actual command handlers)
- **DbContext**: Real in-memory instance (tests persistence)

This hybrid approach:
- Tests service integration (command handlers + controllers)
- Isolates from external dependencies (real database, real services)
- Provides fast, reliable test execution

## Expected Test Results

All 24 integration tests should:
- ✓ Compile without errors
- ✓ Run in < 5 seconds total
- ✓ Pass on every execution (deterministic)
- ✓ Require no external services
- ✓ Provide clear failure messages

## Extending Tests

To add new integration test scenarios:

1. Create new test file in `Features/Tags/`
2. Inherit from `IntegrationTestBase`
3. Override `InitializeAsync()` if custom setup needed
4. Use helper methods: `CreateTag()`, `CreateTagAssociation()`
5. Mock services as needed
6. Follow naming pattern: `[Operation]_[Resource]_[Scenario]_[Outcome]`

Example:
```csharp
public class CustomTagsIntegrationTests : IntegrationTestBase
{
    [Fact]
    public async Task CustomScenario_WithContext_ProducesExpectedResult()
    {
        // Arrange
        var testData = CreateTag("CustomTag", "CustomCategory");
        
        // Act
        var result = await PerformOperation(testData);
        
        // Assert
        Assert.NotNull(result);
    }
}
```

## Documentation

- **E2E Test Scenarios**: See `../../docs/Testing/E2E_TEST_SCENARIOS.md`
- **Tag Service Design**: See tag entity and service documentation
- **Command Handlers**: See `TagAssignmentCommands.cs` for handler implementations

## CI/CD Integration

Add to CI/CD pipeline:

```yaml
- name: Run Integration Tests
  run: dotnet test backend/tests/EHRPlatform.Tests.Integration -v normal
  
- name: Collect Coverage
  run: dotnet test backend/tests/EHRPlatform.Tests.Integration /p:CollectCoverage=true
  
- name: Upload Coverage
  uses: codecov/codecov-action@v3
```

## Troubleshooting

### Tests Fail to Compile
- Ensure all project references are correct
- Verify NuGet packages are installed: `dotnet restore`
- Check that referenced services are built

### Tests Timeout
- Increase test timeout in test properties
- Profile slow tests with xUnit diagnostics
- Check for infinite loops in mock setup

### Database Errors
- Verify SQLite is available
- Check EF Core migrations are applied
- Ensure in-memory database connections are not shared

### Mock Setup Issues
- Verify mock setup matches actual interface
- Use `It.IsAny<>()` for flexible parameter matching
- Check `ReturnsAsync()` for async methods
