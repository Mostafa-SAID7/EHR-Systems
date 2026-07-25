# Clean Architecture Layers

## Architecture Diagram

```
┌─────────────────────────────────┐
│      Presentation Layer         │  Controllers, API Endpoints
├─────────────────────────────────┤
│     Application Layer           │  Use Cases, CQRS, DTOs
├─────────────────────────────────┤
│       Domain Layer              │  Business Logic, Entities
├─────────────────────────────────┤
│    Infrastructure Layer         │  Database, External Services
└─────────────────────────────────┘
```

---

## Layer 1: Domain (Innermost - Business Rules)

```csharp
// Pure business logic - NO dependencies
namespace EHRPlatform.Services.Patient.Domain.Entities;

public class Patient : AuditableEntity
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    
    // Business rule methods (NOT database operations)
    public void UpdateContactInfo(string email, string phone)
    {
        if (string.IsNullOrEmpty(email))
            throw new DomainException("Email is required");
        
        Email = email;
        PhoneNumber = phone;
    }
    
    public void AddAllergy(string allergen, string severity)
    {
        if (string.IsNullOrEmpty(allergen))
            throw new DomainException("Allergen name required");
        
        Allergies.Add(new PatientAllergy { Allergen = allergen, Severity = severity });
    }
}
```

---

## Layer 2: Application (Use Cases & CQRS)

```csharp
// Commands (Write operations)
public class RegisterPatientCommand : ICommand<PatientResponse>
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
}

// Command Handler
public class RegisterPatientCommandHandler : ICommandHandler<RegisterPatientCommand, PatientResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly PatientMapper _mapper;
    
    public async Task<PatientResponse> Handle(RegisterPatientCommand command, CancellationToken ct)
    {
        var patient = new Patient
        {
            FirstName = command.FirstName,
            LastName = command.LastName,
            Email = command.Email
        };
        
        await _unitOfWork.Patients.AddAsync(patient, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        
        return _mapper.MapToResponse(patient);
    }
}

// Queries (Read operations)
public class GetPatientQuery : IQuery<PatientResponse>
{
    public int PatientId { get; set; }
}

public class GetPatientQueryHandler : IQueryHandler<GetPatientQuery, PatientResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly PatientMapper _mapper;
    
    public async Task<PatientResponse> Handle(GetPatientQuery query, CancellationToken ct)
    {
        var patient = await _unitOfWork.Patients.GetByIdAsync(query.PatientId, ct);
        return _mapper.MapToResponse(patient);
    }
}
```

---

## Layer 3: Infrastructure (Data Access)

```csharp
// Data access
public class PatientRepository : IPatientRepository
{
    private readonly DbContext _context;
    
    public async Task<Patient> GetByIdAsync(int id, CancellationToken ct)
    {
        return await _context.Patients.FindAsync(new object[] { id }, cancellationToken: ct);
    }
    
    public async Task AddAsync(Patient patient, CancellationToken ct)
    {
        await _context.Patients.AddAsync(patient, ct);
    }
}

// Database configuration
public class PatientConfiguration : IEntityTypeConfiguration<Patient>
{
    public void Configure(EntityTypeBuilder<Patient> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Email).IsRequired().HasMaxLength(255);
        builder.HasIndex(p => p.Email).IsUnique();
    }
}

// External service
public class EmailService : IEmailService
{
    private readonly HttpClient _httpClient;
    
    public async Task SendAsync(string email, string subject, string body)
    {
        // Call external email service
        await _httpClient.PostAsync("/send", ...);
    }
}
```

---

## Layer 4: Presentation (API Endpoints)

```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PatientsController : ControllerBase
{
    private readonly IMediator _mediator;
    
    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterPatientRequest request)
    {
        var command = new RegisterPatientCommand
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email
        };
        
        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetPatient), new { id = result.Id }, result);
    }
    
    [HttpGet("{id}")]
    public async Task<IActionResult> GetPatient(int id)
    {
        var query = new GetPatientQuery { PatientId = id };
        var result = await _mediator.Send(query);
        return Ok(result);
    }
}
```

---

## Benefits of Clean Architecture

✅ **Testability** - Each layer tested independently  
✅ **Maintainability** - Clear separation of concerns  
✅ **Flexibility** - Swap implementations easily  
✅ **Scalability** - Add new features without affecting existing code  
✅ **Team scalability** - New developers understand structure quickly
