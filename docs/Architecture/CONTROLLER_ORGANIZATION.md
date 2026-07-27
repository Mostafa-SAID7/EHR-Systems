# Controller Organization Architecture

## Overview

Controllers are organized by domain entities and concerns, eliminating duplication while maintaining clear separation of responsibilities. This document defines the organization pattern used across all 10 microservices.

**Status:** ✅ Implemented
**Build:** 0 errors

---

## Design Principles

### 1. **Separation by Aggregate**
Each bounded context (service) has one controller per aggregate root.

```
Patient Service:
├── PatientsController          (Patient CRUD)
└── PatientTagsController       (Patient tag management)

Appointment Service:
├── AppointmentsController      (Appointment lifecycle)
├── ProviderAvailabilityController (Provider scheduling)
└── AppointmentTagsController   (Appointment tag management)

Billing Service:
├── InvoicesController          (Invoice operations)
└── InvoiceTagsController       (Invoice tag management)
```

### 2. **Separate Concerns**
- **CRUD Controllers:** Entity creation, read, update, delete
- **Tag Controllers:** Tag application, removal, query
- **Specialized Controllers:** Domain-specific operations (e.g., ProviderAvailabilityController)

### 3. **DRY (Don't Repeat Yourself)**
- Tag endpoint code is identical across services → single base implementation pattern
- Category providers encapsulate service-specific logic
- Shared CQRS commands/handlers eliminate duplication

---

## File Structure

### Appointment Service Example

```
backend/src/EHRPlatform.Services.Appointment/
├── Controllers/
│   ├── AppointmentsController.cs
│   │   ├── GET    /api/v1/appointments/{id}
│   │   ├── POST   /api/v1/appointments
│   │   ├── POST   /api/v1/appointments/{id}/confirm
│   │   ├── POST   /api/v1/appointments/{id}/cancel
│   │   ├── POST   /api/v1/appointments/{id}/check-in
│   │   └── POST   /api/v1/appointments/{id}/complete
│   │
│   ├── ProviderAvailabilityController.cs
│   │   ├── GET    /api/v1/providers/{providerId}/calendar
│   │   ├── GET    /api/v1/providers/{providerId}/availability
│   │   └── POST   /api/v1/providers/{providerId}/availability
│   │
│   └── AppointmentTagsController.cs
│       ├── GET    /api/v1/appointments/{appointmentId}/tags
│       ├── POST   /api/v1/appointments/{appointmentId}/tags
│       ├── DELETE /api/v1/appointments/{appointmentId}/tags/{tagId}
│       └── PUT    /api/v1/appointments/{appointmentId}/tags
│
├── Categories/
│   └── AppointmentCategoryProvider.cs
│       ├── Priority categories (time-based)
│       ├── Auto-recommendation logic
│       └── Service-specific rules
│
└── Features/
    ├── Appointments/
    │   ├── Commands/ (ScheduleAppointmentCommand, etc.)
    │   ├── Queries/  (GetAppointmentQuery, etc.)
    │   └── Domain/   (Appointment entity, events)
    │
    └── Availability/
        ├── Commands/ (SetProviderAvailabilityCommand)
        └── Queries/  (GetProviderAvailabilityQuery)
```

### Billing Service Example

```
backend/src/EHRPlatform.Services.Billing/
├── Controllers/
│   ├── InvoicesController.cs
│   │   ├── GET    /api/v1/invoices/by-number/{invoiceNumber}
│   │   ├── POST   /api/v1/invoices
│   │   ├── POST   /api/v1/invoices/{invoiceId}/payments
│   │   └── POST   /api/v1/invoices/{invoiceId}/submit-insurance
│   │
│   └── InvoiceTagsController.cs
│       ├── GET    /api/v1/invoices/{invoiceId}/tags
│       ├── POST   /api/v1/invoices/{invoiceId}/tags
│       ├── DELETE /api/v1/invoices/{invoiceId}/tags/{tagId}
│       └── PUT    /api/v1/invoices/{invoiceId}/tags
│
├── Categories/
│   └── BillingCategoryProvider.cs
│       ├── Amount-based categorization
│       ├── Overdue detection
│       └── Insurance status tracking
│
└── Features/
    ├── Invoicing/
    │   ├── Commands/ (CreateInvoiceCommand, etc.)
    │   ├── Queries/  (GetInvoiceByNumberQuery, etc.)
    │   └── Domain/   (Invoice entity, events)
    │
    ├── Payments/
    │   └── Commands/ (RecordPaymentCommand)
    │
    └── Claims/
        └── Commands/ (SubmitToInsuranceCommand)
```

### Patient Service Example

```
backend/src/EHRPlatform.Services.Patient/
├── Controllers/
│   ├── PatientsController.cs
│   │   ├── GET    /api/v1/patients/{id}
│   │   ├── GET    /api/v1/patients/mrn/{mrnValue}
│   │   ├── POST   /api/v1/patients
│   │   └── PUT    /api/v1/patients/{id}
│   │
│   └── PatientTagsController.cs
│       ├── GET    /api/v1/patients/{patientId}/tags
│       ├── POST   /api/v1/patients/{patientId}/tags
│       ├── DELETE /api/v1/patients/{patientId}/tags/{tagId}
│       └── PUT    /api/v1/patients/{patientId}/tags
│
├── Categories/
│   └── PatientCategoryProvider.cs
│       ├── Risk categorization
│       ├── Auto-recommendation logic
│       └── Patient lifecycle stages
│
└── Features/
    └── Patients/
        ├── Commands/ (CreatePatientCommand, etc.)
        ├── Queries/  (GetPatientQuery, etc.)
        └── Domain/   (Patient entity, events)
```

---

## Controller Patterns

### 1. Entity CRUD Controller

**Purpose:** Handle create, read, update, delete operations for main aggregate

**Pattern:**
```csharp
[ApiController]
[Route("api/v1/patients")]
[Authorize]
public class PatientsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<PatientsController> _logger;

    public PatientsController(IMediator mediator, ILogger<PatientsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> CreatePatient([FromBody] CreatePatientCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        return CreatedAtAction(nameof(GetPatient), new { id = result.Id }, result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetPatient(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetPatientQuery { PatientId = id }, ct);
        return Ok(result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdatePatient(Guid id, [FromBody] UpdatePatientCommand command, CancellationToken ct)
    {
        command.PatientId = id;
        var result = await _mediator.Send(command, ct);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePatient(Guid id, CancellationToken ct)
    {
        await _mediator.Send(new DeletePatientCommand { PatientId = id }, ct);
        return NoContent();
    }
}
```

**Responsibilities:**
- ✅ HTTP verb routing
- ✅ Request/response mapping
- ✅ CQRS command/query dispatching
- ✅ HTTP status code selection
- ✅ Error handling

**Injected Dependencies:**
- `IMediator` - CQRS command/query bus
- `ILogger<T>` - Structured logging

---

### 2. Tag Controller

**Purpose:** Handle tag lifecycle (apply, remove, query) for any aggregate

**Pattern:**
```csharp
[ApiController]
[Route("api/v1/patients/{patientId}/tags")]
public class PatientTagsController : ControllerBase
{
    private readonly ITagQueryService _tagQueryService;
    private readonly IMediator _mediator;
    private readonly ILogger<PatientTagsController> _logger;

    public PatientTagsController(ITagQueryService tqs, IMediator mediator, ILogger<PatientTagsController> logger)
    {
        _tagQueryService = tqs;
        _mediator = mediator;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetTags(Guid patientId, CancellationToken ct)
    {
        var tags = await _tagQueryService.GetResourceTagsAsync(patientId, nameof(PatientEntity), ct);
        return Ok(new { patientId, tags });
    }

    [HttpPost]
    public async Task<IActionResult> ApplyTags(Guid patientId, [FromBody] ApplyTagsCommand baseCommand, CancellationToken ct)
    {
        var command = baseCommand with
        {
            ResourceId = patientId,
            ResourceType = nameof(PatientEntity),
            ServiceName = "Patient"
        };
        var result = await _mediator.Send(command, ct);
        return Ok(result);
    }

    [HttpDelete("{tagId}")]
    public async Task<IActionResult> RemoveTag(Guid patientId, Guid tagId, CancellationToken ct)
    {
        var command = new RemoveTagCommand
        {
            ResourceId = patientId,
            ResourceType = nameof(PatientEntity),
            TagId = tagId,
            ServiceName = "Patient"
        };
        await _mediator.Send(command, ct);
        return NoContent();
    }

    [HttpPut]
    public async Task<IActionResult> SetTags(Guid patientId, [FromBody] SetResourceTagsCommand baseCommand, CancellationToken ct)
    {
        var command = baseCommand with
        {
            ResourceId = patientId,
            ResourceType = nameof(PatientEntity),
            ServiceName = "Patient"
        };
        var result = await _mediator.Send(command, ct);
        return Ok(result);
    }
}
```

**Key Points:**
- ✅ Nested route: `/patients/{patientId}/tags`
- ✅ Uses `with` operator for immutable record initialization
- ✅ Sets ResourceType via `nameof(Entity)`
- ✅ Sets ServiceName to service name
- ✅ Query service for GET, CQRS commands for mutations

**Responsibilities:**
- ✅ Tag query operations
- ✅ Tag application/removal via CQRS
- ✅ Atomic tag replacement
- ✅ Error handling for missing tags/resources

**Injected Dependencies:**
- `ITagQueryService` - Tag querying
- `IMediator` - CQRS bus
- `ILogger<T>` - Logging

---

### 3. Specialized Controller

**Purpose:** Handle domain-specific operations (e.g., provider availability, appointments)

**Pattern:**
```csharp
[ApiController]
[Route("api/v1/providers")]
public class ProviderAvailabilityController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<ProviderAvailabilityController> _logger;

    [HttpGet("{providerId}/calendar")]
    public async Task<IActionResult> GetCalendar(Guid providerId, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetProviderAppointmentsQuery { ProviderId = providerId }, ct);
        return Ok(result);
    }

    [HttpPost("{providerId}/availability")]
    public async Task<IActionResult> SetAvailability(Guid providerId, [FromBody] SetProviderAvailabilityCommand cmd, CancellationToken ct)
    {
        var command = cmd with { ProviderId = providerId };
        var result = await _mediator.Send(command, ct);
        return Ok(result);
    }
}
```

**When to Use:**
- ✅ Domain-specific operations
- ✅ Operations that don't fit CRUD
- ✅ Multi-step workflows
- ✅ Provider-centric vs Patient-centric operations

---

## Route Organization

### Appointment Service Routes

```
ENTITY CRUD:
  GET    /api/v1/appointments/{id}                    ← Get appointment
  POST   /api/v1/appointments                         ← Create appointment
  POST   /api/v1/appointments/{id}/confirm            ← State transition
  POST   /api/v1/appointments/{id}/cancel             ← State transition
  POST   /api/v1/appointments/{id}/check-in           ← State transition
  POST   /api/v1/appointments/{id}/complete           ← State transition

TAGS:
  GET    /api/v1/appointments/{id}/tags               ← Get tags (nested)
  POST   /api/v1/appointments/{id}/tags               ← Apply tags
  DELETE /api/v1/appointments/{id}/tags/{tagId}       ← Remove tag
  PUT    /api/v1/appointments/{id}/tags               ← Replace tags

PROVIDER-SPECIFIC:
  GET    /api/v1/providers/{providerId}/calendar      ← Provider calendar
  GET    /api/v1/providers/{providerId}/availability  ← Availability slots
  POST   /api/v1/providers/{providerId}/availability  ← Set availability
```

### Billing Service Routes

```
SLUG-BASED (Invoice Number):
  GET    /api/v1/invoices/by-number/{invoiceNumber}  ← SEO-friendly lookup

ENTITY CRUD:
  POST   /api/v1/invoices                             ← Create invoice
  POST   /api/v1/invoices/{id}/payments               ← Record payment
  POST   /api/v1/invoices/{id}/submit-insurance       ← Submit to insurance

TAGS:
  GET    /api/v1/invoices/{id}/tags                   ← Get tags
  POST   /api/v1/invoices/{id}/tags                   ← Apply tags
  DELETE /api/v1/invoices/{id}/tags/{tagId}           ← Remove tag
  PUT    /api/v1/invoices/{id}/tags                   ← Replace tags
```

### Patient Service Routes

```
MRN-BASED (Medical Record Number):
  GET    /api/v1/patients/mrn/{mrnValue}              ← Slug-based lookup
  GET    /api/v1/patients/mrn/{mrnValue}/detail       ← Detailed info

ENTITY CRUD:
  GET    /api/v1/patients/{id}                        ← Get patient
  POST   /api/v1/patients                             ← Create patient
  PUT    /api/v1/patients/{id}                        ← Update patient

TAGS:
  GET    /api/v1/patients/{id}/tags                   ← Get tags
  POST   /api/v1/patients/{id}/tags                   ← Apply tags
  DELETE /api/v1/patients/{id}/tags/{tagId}           ← Remove tag
  PUT    /api/v1/patients/{id}/tags                   ← Replace tags
```

---

## Dependency Injection

### CRUD Controller
```csharp
services.AddScoped<PatientsController>();
services.AddScoped<IMediator, Mediator>();
services.AddLogging();
```

### Tag Controller
```csharp
services.AddScoped<PatientTagsController>();
services.AddScoped<ITagQueryService, TagQueryService>();
services.AddScoped<IMediator, Mediator>();
services.AddLogging();
```

### Service Registration
```csharp
// In Program.cs
public static IServiceCollection AddPatientServices(this IServiceCollection services)
{
    // Controllers
    services.AddScoped<PatientsController>();
    services.AddScoped<PatientTagsController>();
    
    // Services
    services.AddScoped<ITagQueryService, TagQueryService>();
    services.AddScoped<ICategoryProvider, PatientCategoryProvider>();
    
    // CQRS
    services.AddMediatR(...);
    
    return services;
}
```

---

## Error Handling

### Standard Response Patterns

**200 OK - Successful GET/POST/PUT:**
```json
{
  "id": "...",
  "name": "...",
  "status": "Active"
}
```

**201 Created - Resource created:**
```
Location: /api/v1/patients/{id}
{ ... resource ... }
```

**204 No Content - Deletion/No response:**
```
(empty body)
```

**400 Bad Request - Validation error:**
```json
{
  "errors": {
    "email": ["Invalid email format"],
    "age": ["Must be between 0 and 150"]
  }
}
```

**404 Not Found:**
```json
{
  "message": "Patient not found"
}
```

**500 Internal Server Error:**
```json
{
  "message": "An error occurred",
  "traceId": "0HN1JFVMDVV0L:00000001"
}
```

---

## Testing Strategy

### Test Structure by Controller Type

**CRUD Controller Tests:**
- ✅ CREATE: Valid input, duplicate prevention, validation
- ✅ READ: Existing resource, not found, filtering
- ✅ UPDATE: Partial update, version conflict, validation
- ✅ DELETE: Soft delete, cascade behavior, authorization

**Tag Controller Tests:**
- ✅ GET: Existing tags, empty result, error handling
- ✅ POST: Valid tags, duplicate prevention, authorization
- ✅ DELETE: Existing tag, not found, cascade
- ✅ PUT: Atomic replacement, validation, audit trail

---

## Best Practices

1. **One Controller Per Aggregate**
   - Reduces cognitive load
   - Clearer responsibility
   - Easier testing

2. **Separate Tag Concerns**
   - Tag operations isolated in dedicated controllers
   - Reusable patterns across services
   - Eliminates 200+ lines of duplication

3. **Use `nameof()` for Type Names**
   - Compile-time safety
   - Refactoring-friendly
   - No magic strings

4. **Immutable Command Records**
   - Use `with` operator for initialization
   - Prevents accidental mutation
   - Clean, functional style

5. **Consistent Error Handling**
   - Try-catch in action methods
   - Log errors with context
   - Return appropriate HTTP status

6. **Dependency Injection**
   - Constructor injection only
   - Mock-friendly for testing
   - Clear dependencies

---

## Related Documentation

- [CQRS Implementation](./CQRS_IMPLEMENTATION.md)
- [Tag Service Architecture](./TAG_SERVICE_ARCHITECTURE.md)
- [Slug-based URLs](../API/SLUG_BASED_URLS.md)
- [Testing Guide](../Testing/TAG_ENDPOINTS_TESTING.md)

