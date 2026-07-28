# End-to-End Testing

End-to-end tests for complete business workflows spanning multiple microservices.

## Test Categories

### Scenarios (`Scenarios/`)
Complete business workflow tests:
- Patient onboarding workflow
- Appointment booking and management
- Clinical record creation and updates
- Prescription ordering and fulfillment
- Billing and payment processing
- Report generation
- Notification delivery

### Integrations (`Integrations/`)
Cross-service integration validation:
- Service-to-service communication
- Event propagation and handling
- Data consistency across services
- Transaction coordination
- Error handling and recovery
- Fallback mechanisms

### Test Data (`TestData/`)
Factories and builders for E2E test data:
- Patient data factories
- Clinical record templates
- Appointment templates
- Prescription generators
- User and role fixtures
- Database seeding utilities

## Running E2E Tests

```bash
# Run all E2E tests
dotnet test tests/EHRPlatform.Tests.E2E/EHRPlatform.Tests.E2E.csproj

# Run specific scenario
dotnet test tests/EHRPlatform.Tests.E2E/EHRPlatform.Tests.E2E.csproj --filter "FullyQualifiedName~PatientOnboardingScenarioTests"

# Run with detailed output
dotnet test tests/EHRPlatform.Tests.E2E/EHRPlatform.Tests.E2E.csproj -v detailed

# Run with test data persistence (for debugging)
dotnet test tests/EHRPlatform.Tests.E2E/EHRPlatform.Tests.E2E.csproj --no-cleanup
```

## Environment Setup

E2E tests use Docker Compose to set up the complete environment:

```bash
# Start test environment
docker-compose -f docker-compose.test.yml up -d

# Run tests
dotnet test tests/EHRPlatform.Tests.E2E/EHRPlatform.Tests.E2E.csproj

# Cleanup
docker-compose -f docker-compose.test.yml down
```

## Test Data Management

### Creating Test Data
```csharp
var patient = await PatientFactory.CreatePatientAsync(
    name: "John Doe",
    email: "john@test.com",
    dateOfBirth: DateTime.Parse("1980-01-01")
);
```

### Seeding Database
```csharp
await TestDataSeeder.SeedDefaultUsersAsync();
await TestDataSeeder.SeedDefaultPatientsAsync(count: 10);
await TestDataSeeder.SeedDefaultAppointmentsAsync(count: 5);
```

### Cleanup
```csharp
await TestDataCleaner.DeleteAllPatientsAsync();
await TestDataCleaner.DeleteAllAppointmentsAsync();
```

## Typical E2E Test Flow

1. **Setup**: Initialize test environment and authenticate users
2. **Arrange**: Create initial test data (patients, appointments, etc.)
3. **Act**: Execute business workflow (API calls, event publishing)
4. **Assert**: Verify workflow outcome and side effects
5. **Cleanup**: Delete test data and verify cleanup

## Example E2E Test

```csharp
public class PatientOnboardingScenarioTests : E2ETestBase
{
    private readonly PatientFactory _patientFactory;
    private readonly HttpClient _httpClient;

    public PatientOnboardingScenarioTests()
    {
        _patientFactory = new PatientFactory();
        _httpClient = new HttpClient { BaseAddress = new Uri("http://localhost:5000") };
    }

    [Fact]
    public async Task CompletePatientOnboarding_WithValidData_SucceedsEndToEnd()
    {
        // Arrange
        var patientData = _patientFactory.CreatePatientRequest();

        // Act
        var registrationResponse = await PostAsync<PatientDto>("/api/v1/patients", patientData);
        var verificationResponse = await PostAsync<object>(
            $"/api/v1/patients/{registrationResponse.Id}/verify", 
            new { code = "123456" }
        );

        // Assert
        Assert.NotNull(registrationResponse);
        Assert.NotNull(verificationResponse);
        
        // Verify side effects
        var auditEntry = await GetAsync<AuditEntryDto>($"/api/v1/audit/{registrationResponse.Id}");
        Assert.NotNull(auditEntry);
        Assert.Equal("PatientRegistered", auditEntry.EventType);
    }
}
```

## Test Execution Order

Tests are grouped by service dependency order:
1. Identity Service tests (authentication foundation)
2. Patient Service tests (base data)
3. Clinical Service tests (clinical records)
4. Appointment Service tests
5. Prescription Service tests
6. Billing Service tests
7. Cross-service integration tests

## Debugging E2E Tests

### Viewing Service Logs
```bash
docker-compose -f docker-compose.test.yml logs -f patient-service
```

### Inspecting Database State
```bash
docker exec -it test-postgres psql -U ehr_user -d ehr_db -c "SELECT * FROM patients"
```

### Increasing Timeout for Debugging
```csharp
_httpClient.Timeout = TimeSpan.FromMinutes(10); // For debugging
```

## Performance Considerations

- E2E tests are slower than unit tests (~5-10 seconds per test)
- Run only critical paths in E2E
- Use contract tests for provider validation
- Use integration tests for service interaction
- Reserve E2E for complete workflows

## Continuous Integration

E2E tests run:
- On pull requests (subset of tests)
- On commits to main (full test suite)
- Nightly (with extended scenarios)
- Before releases (critical workflows)

## Troubleshooting

### Tests Fail with Connection Refused
- Verify services are running: `docker ps`
- Check service health: `curl http://localhost:5000/health`
- View logs: `docker-compose logs -f`

### Tests Timeout
- Increase timeout in fixture
- Check database query performance
- Look for service-to-service communication issues

### Flaky Tests
- Add explicit waits for async operations
- Verify service startup times
- Check for proper test isolation

## Best Practices

1. **Keep Tests Focused**: One business workflow per test
2. **Use Factories**: Create consistent test data
3. **Verify Side Effects**: Check audit logs, events, etc.
4. **Clean Up**: Always delete test data
5. **Document**: Explain complex workflows
6. **Realistic Data**: Use realistic patient/clinical data patterns
7. **Idempotency**: Tests should be rerunnable
