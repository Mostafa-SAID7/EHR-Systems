# Unit Testing Patterns

## AAA Pattern (Arrange, Act, Assert)

```csharp
[Fact]
public async Task CreateUser_WithValidEmail_ReturnsUser()
{
    // ARRANGE - Setup
    var mockRepository = new Mock<IUserRepository>();
    var service = new UserService(mockRepository.Object);
    var request = new CreateUserRequest { Email = "user@example.com", Name = "Ahmed" };
    
    // ACT - Execute
    var result = await service.CreateUserAsync(request);
    
    // ASSERT - Verify
    Assert.NotNull(result);
    Assert.Equal("user@example.com", result.Email);
    mockRepository.Verify(r => r.AddAsync(It.IsAny<User>()), Times.Once);
}
```

---

## Testing Commands

```csharp
public class RegisterPatientCommandHandlerTests
{
    [Fact]
    public async Task Handle_WithValidData_SavesPatient()
    {
        // Arrange
        var mockUnitOfWork = new Mock<IUnitOfWork>();
        var mockMapper = new Mock<IMapper>();
        
        var handler = new RegisterPatientCommandHandler(mockUnitOfWork.Object, mockMapper.Object);
        var command = new RegisterPatientCommand
        {
            FirstName = "Ahmed",
            LastName = "Hassan",
            Email = "ahmed@example.com"
        };
        
        var patient = new Patient { Id = 1, FirstName = "Ahmed" };
        mockMapper.Setup(m => m.Map<Patient>(command)).Returns(patient);
        
        // Act
        var result = await handler.Handle(command, CancellationToken.None);
        
        // Assert
        mockUnitOfWork.Verify(u => u.Patients.AddAsync(patient, It.IsAny<CancellationToken>()), Times.Once);
        mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
```

---

## Testing Queries

```csharp
public class GetPatientQueryHandlerTests
{
    [Fact]
    public async Task Handle_WithValidId_ReturnsPatient()
    {
        // Arrange
        var mockUnitOfWork = new Mock<IUnitOfWork>();
        var mockMapper = new Mock<IMapper>();
        
        var patient = new Patient { Id = 1, FirstName = "Ahmed" };
        var patientDto = new PatientResponse { Id = 1, FirstName = "Ahmed" };
        
        mockUnitOfWork
            .Setup(u => u.Patients.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(patient);
        
        mockMapper.Setup(m => m.Map<PatientResponse>(patient)).Returns(patientDto);
        
        var handler = new GetPatientQueryHandler(mockUnitOfWork.Object, mockMapper.Object);
        var query = new GetPatientQuery { PatientId = 1 };
        
        // Act
        var result = await handler.Handle(query, CancellationToken.None);
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal("Ahmed", result.FirstName);
    }
}
```

---

## Testing Validation

```csharp
public class CreateUserValidatorTests
{
    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Validate_WithEmptyEmail_Fails(string email)
    {
        // Arrange
        var validator = new CreateUserValidator();
        var request = new CreateUserRequest { Email = email };
        
        // Act
        var result = validator.Validate(request);
        
        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Email");
    }
}
```

---

## Integration Testing

```csharp
public class PatientServiceIntegrationTests : IAsyncLifetime
{
    private IServiceScope _scope;
    private DbContext _context;
    private IUserService _service;
    
    public async Task InitializeAsync()
    {
        // Setup database
        var services = new ServiceCollection();
        services.AddDbContext<DbContext>(opt => opt.UseInMemoryDatabase("test"));
        services.AddScoped<IUserService, UserService>();
        
        var provider = services.BuildServiceProvider();
        _scope = provider.CreateAsyncScope();
        _context = _scope.ServiceProvider.GetRequiredService<DbContext>();
        _service = _scope.ServiceProvider.GetRequiredService<IUserService>();
        
        await _context.Database.EnsureCreatedAsync();
    }
    
    [Fact]
    public async Task CreateUser_WithValidData_SavesToDatabase()
    {
        // Act
        var user = await _service.CreateUserAsync(new CreateUserRequest { Email = "user@test.com" });
        
        // Assert
        var saved = await _context.Users.FirstOrDefaultAsync(u => u.Email == "user@test.com");
        Assert.NotNull(saved);
    }
    
    public async Task DisposeAsync()
    {
        await _context.Database.EnsureDeletedAsync();
        await _scope.DisposeAsync();
    }
}
```

---

## Interview Q&A

**Q: Unit vs Integration tests?**

A:
- Unit: Test single method with mocks (fast, isolated)
- Integration: Test multiple components together (slow, realistic)

**Q: How many tests?**

A: Pyramid approach:
- Many unit tests (80%)
- Some integration tests (15%)
- Few end-to-end tests (5%)
