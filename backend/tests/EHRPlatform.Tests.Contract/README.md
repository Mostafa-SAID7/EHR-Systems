# Contract Testing

Contract testing for microservice interfaces ensuring compatibility between services and external providers.

## Test Categories

### Service Contracts (`Services/`)
Consumer-driven contract tests for inter-service communication:
- Identity Service API contracts
- Patient Service API contracts
- Clinical Service API contracts
- Appointment Service API contracts
- Prescription Service API contracts
- Billing Service API contracts
- Notification Service API contracts
- Analytics Service API contracts
- Audit Service API contracts

### Provider Contracts (`Providers/`)
External service provider contract tests:
- Payment gateway integration (Stripe, Square)
- Insurance validation services
- Pharmacy benefit managers
- Laboratory information systems
- Imaging center integrations
- Government reporting systems

### HIPAA Compliance (`HipaaCompliance/`)
HIPAA compliance contract validation:
- PHI/PII protection validation
- Access control contracts
- Audit trail contracts
- Data encryption contracts

## Running Contract Tests

```bash
# Run all contract tests
dotnet test tests/EHRPlatform.Tests.Contract/EHRPlatform.Tests.Contract.csproj

# Run specific service contracts
dotnet test tests/EHRPlatform.Tests.Contract/EHRPlatform.Tests.Contract.csproj --filter "FullyQualifiedName~PatientServiceContractTests"

# Run with detailed output
dotnet test tests/EHRPlatform.Tests.Contract/EHRPlatform.Tests.Contract.csproj -v detailed

# Run provider contracts only
dotnet test tests/EHRPlatform.Tests.Contract/EHRPlatform.Tests.Contract.csproj --filter "FullyQualifiedName~Providers"
```

## Contract Test Structure

### Consumer-Driven Contracts
Test how services expect providers to behave:

```csharp
[Fact]
public async Task PatientServiceConsumer_ExpectsPatientServiceToReturnValidPatient()
{
    // Arrange
    var patientId = "patient-123";
    var expectedContract = new PatientResponse 
    { 
        Id = patientId, 
        Name = "John Doe" 
    };

    // Act
    var response = await _httpClient.GetAsync($"/api/v1/patients/{patientId}");
    
    // Assert
    Assert.True(response.IsSuccessStatusCode);
    var content = await response.Content.ReadAsAsync<PatientResponse>();
    Assert.Equal(expectedContract.Id, content.Id);
}
```

### Provider Contracts
Test external service integration expectations:

```csharp
[Fact]
public async Task StripePaymentProvider_ProcessesPaymentCorrectly()
{
    // Arrange
    var paymentRequest = new PaymentRequest 
    { 
        Amount = 100.00m, 
        Currency = "USD" 
    };

    // Act
    var response = await _stripeClient.ChargeAsync(paymentRequest);

    // Assert
    Assert.NotNull(response.TransactionId);
    Assert.Equal(PaymentStatus.Completed, response.Status);
}
```

## Contract Versioning

Contracts are versioned to track compatibility:

```
v1.0: Initial patient service contract
v1.1: Added insurance identifier field
v2.0: Breaking change - removed legacy identifier field
```

## Breaking Changes

When a breaking change is required:

1. Increment major version (v1.0 → v2.0)
2. Update all consumers
3. Run full test suite
4. Coordinate deployment with consumers
5. Maintain backward compatibility window (if possible)

## Testing External Providers

### Using Testcontainers
```csharp
public class ExternalProviderContractTests : IAsyncLifetime
{
    private PostgreSqlContainer _container;

    public async Task InitializeAsync()
    {
        _container = new PostgreSqlBuilder().Build();
        await _container.StartAsync();
    }

    public async Task DisposeAsync()
    {
        await _container.StopAsync();
    }
}
```

### Using Mock Servers
```csharp
public class PaymentProviderContractTests
{
    private MockServer _mockServer;

    [SetUp]
    public void Setup()
    {
        _mockServer = new MockServer(8888);
        _mockServer.When(HttpRequest.GET("/payment/status"))
            .Respond(HttpResponse.OK().WithBody(JsonConvert.SerializeObject(statusResponse)));
    }
}
```

## HIPAA Compliance in Contracts

Contracts must ensure:

1. **PHI Protection**: Patient health information is encrypted/redacted
2. **PII Protection**: Personally identifiable information is protected
3. **Access Control**: Only authorized services can access data
4. **Audit Logging**: All access is logged
5. **Data Retention**: Data is retained per policy

### Example HIPAA-Compliant Contract

```csharp
[Fact]
public async Task PatientServiceContract_PhiIsEncrypted()
{
    // Arrange & Act
    var response = await GetPatientAsync("patient-123");
    
    // Assert
    Assert.NotEmpty(response.MedicalRecordNumber); // Should be encrypted
    Assert.DoesNotContain("@", response.Email); // Email should be redacted
    Assert.True(response.IsEncrypted);
}
```

## Contract Verification Flow

1. **Define Contract**: Specify expected request/response format
2. **Mock Provider**: Create mock implementation matching contract
3. **Test Consumer**: Verify consumer works with mock
4. **Verify Provider**: Verify actual provider implements contract
5. **Record**: Save contract for future verification

## CI/CD Integration

Contract tests run:
- On pull requests (contract changes reviewed)
- On commits to main (ensure contracts remain valid)
- Nightly (deep contract validation)
- Before releases (contract compatibility verified)

## Contract Maintenance

### Updating Contracts
1. Identify required change
2. Update contract definition
3. Update test
4. Update service implementation
5. Coordinate with consumers
6. Deploy changes

### Backward Compatibility
```csharp
[Fact]
public async Task LegacyContract_IsStillSupported()
{
    // Test v1 format still works
    var legacyResponse = await CallWithLegacyFormat();
    Assert.NotNull(legacyResponse);
    
    // Test v2 format also works
    var newResponse = await CallWithNewFormat();
    Assert.NotNull(newResponse);
}
```

## Example Contracts

### Patient Service Response Contract
```json
{
  "id": "patient-123",
  "firstName": "John",
  "lastName": "Doe",
  "dateOfBirth": "1980-01-01",
  "gender": "M",
  "email": "john@example.com",
  "phone": "+1-555-123-4567",
  "address": {
    "street": "123 Main St",
    "city": "Springfield",
    "state": "IL",
    "zip": "62701"
  }
}
```

### Appointment Service Request Contract
```json
{
  "patientId": "patient-123",
  "providerId": "provider-456",
  "startTime": "2024-01-15T10:00:00Z",
  "endTime": "2024-01-15T10:30:00Z",
  "type": "consultation",
  "notes": "Initial consultation"
}
```

## Troubleshooting

### Contract Mismatch
- Review contract definition
- Check service implementation
- Verify serialization settings
- Compare actual vs expected format

### Provider Integration Issues
- Verify provider credentials
- Check network connectivity
- Review provider documentation
- Enable provider debug logging

### Backward Compatibility Problems
- Maintain legacy endpoint
- Support both request formats
- Provide migration period
- Communicate deprecation clearly
