# Integration & E2E Testing Strategies

Comprehensive testing patterns covering unit tests, integration testing with Testcontainers, and end-to-end API testing in .NET.

---

## 1. Unit Testing Best Practices (AAA Pattern & Moq)

```csharp
public class AppointmentServiceTests
{
    private readonly Mock<IAppointmentRepository> _repoMock = new();
    private readonly AppointmentService _service;

    public AppointmentServiceTests()
    {
        _service = new AppointmentService(_repoMock.Object);
    }

    [Fact]
    public async Task ScheduleAppointmentAsync_ValidRequest_ReturnsScheduledAppointment()
    {
        // Arrange
        var request = new ScheduleAppointmentCommand(PatientId: 10, DoctorId: 5, Slot: DateTime.UtcNow.AddDays(1));
        _repoMock.Setup(r => r.HasOverlapAsync(request.DoctorId, request.Slot)).ReturnsAsync(false);

        // Act
        var result = await _service.ScheduleAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(AppointmentStatus.Scheduled, result.Status);
        _repoMock.Verify(r => r.SaveAsync(It.IsAny<Appointment>()), Times.Once);
    }
}
```

---

## 2. Integration Testing with `WebApplicationFactory` & Testcontainers

```csharp
public class PatientsApiIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public PatientsApiIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetPatientById_ExistingId_Returns200OK()
    {
        var response = await _client.GetAsync("/api/v1/patients/10");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
```
