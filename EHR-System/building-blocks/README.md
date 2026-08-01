# EHR Platform - Building Blocks

Shared libraries and cross-cutting concerns for all microservices in the EHR platform.

Building-blocks eliminate code duplication by providing reusable components for authentication, observability, data access, events, and utilities.

## 📦 Packages Overview

### 1. **Common** - Core Utilities & Extensions
String, Enum, and Collection manipulation helpers for healthcare data processing.

**Key Classes:**
- `StringExtensions` - Slug generation, validation (email, phone, MRN), truncation, masking for logging
- `EnumExtensions` - Display names, descriptions, parsing with fallback to enum names
- `CollectionExtensions` - Batch processing, pagination, distinct, chunking, grouping

**Usage:**
```csharp
// String utilities
var slug = "Clinical Note".ToSlug(); // "clinical-note"
var masked = "user@example.com".MaskSensitive(1); // "u***@example.com"
var isValidEmail = "john@example.com".IsValidEmail(); // true

// Enum utilities
var displayName = PatientStatus.Active.GetDisplayName(); // from DisplayAttribute
var allRoles = EnumExtensions.GetValueDictionary<UserRole>();

// Collection utilities
var batches = items.Batch(100); // Process in batches of 100
var (data, total, page, size) = patients.Paginate(pageNumber: 1, pageSize: 10);
var distinct = records.DistinctBy(r => r.PatientId);
```

---

### 2. **SharedKernel** - Domain-Driven Design Foundations
Base classes, value objects, and patterns for enterprise domain modeling.

**Key Classes:**
- `BaseEntity` - Audit fields (CreatedAt, UpdatedBy, DeletedAt), soft deletes, correlation tracking
- `ValueObject` - Immutable, compared by value (equality implementation)
- `Specification<T>` - DDD pattern for encapsulating query logic
- `Result<T>` - Functional error handling (instead of exceptions)
- **Value Objects:**
  - `EmailAddress` - Validation, local/domain extraction
  - `PhoneNumber` - International format support
  - `Address` - Multi-field validation

**Usage:**
```csharp
// Value objects with validation
var emailResult = EmailAddress.Create("john@example.com");
if (emailResult.IsSuccess)
{
    var email = emailResult.Value;
    var domain = email.GetDomain(); // "example.com"
}

// Result pattern (functional error handling)
var result = Address.Create(street, city, state, postal, country);
var address = result.GetValueOrThrow(); // Throws if failed

// Specification pattern for queries
var spec = new GetActivePatientsByNameSpecification("John");
var patients = await repository.GetAsync(spec);
```

---

### 3. **Contracts** - API Contracts & Data Transfer Objects

Standardized request/response models for REST APIs across all services.

**Key Classes:**
- `ApiResponse<T>` - Consistent response envelope with success/error handling
- `PaginatedResponse<T>` - Standard pagination with HasNextPage, TotalPages
- `BaseDto` - Audit fields for responses
- `HealthCheckResponse` - Service health status
- **Request Models:**
  - `CreateRequest` - Base for POST requests
  - `UpdateRequest` - Base for PUT/PATCH requests
  - `SearchRequest` - Base for GET list requests (with pagination)
- **DTOs:**
  - `PatientDto` - Cross-service patient reference

**Usage:**
```csharp
// Successful response with data
var response = ApiResponse<PatientDto>.Ok(patient, "Patient retrieved", traceId);

// Error responses
var notFound = ApiResponse<PatientDto>.NotFound("Patient not found", traceId);
var validationError = ApiResponse<PatientDto>.ValidationError(errors, traceId);

// Paginated responses
var paginated = ApiResponse<PatientDto>.Ok(
    items: patients,
    totalCount: 150,
    pageNumber: 1,
    pageSize: 10
);

// In controller
[HttpGet]
public async Task<ApiResponse<PaginatedResponse<PatientDto>>> GetPatients(
    [FromQuery] SearchPatientRequest request)
{
    request.Validate();
    var spec = new GetPatientsSpecification(request.Search, request.SortBy);
    var (items, total) = await _repository.GetPaginatedAsync(spec, request.PageNumber, request.PageSize);
    return ApiResponse<PaginatedResponse<PatientDto>>.Ok(
        items: _mapper.Map<List<PatientDto>>(items),
        totalCount: total,
        pageNumber: request.PageNumber,
        pageSize: request.PageSize
    );
}
```

---

### 4. **Security** - Authentication & Authorization

JWT tokens, password hashing, encryption, and role-based access control.

**Key Classes:**
- `JwtSettings` - Configuration (issuer, audience, expiration, algorithm)
- `JwtTokenProvider` - Token generation and validation
- `EncryptionService` - Password hashing (bcrypt), token generation, AES-256 encryption
- `AuthorizationPolicies` - Role-based access control (RBAC)
- `ICurrentUserService` - Access current authenticated user context
- **Roles:**
  - `Admin` - System administrators
  - `Clinician` - Doctors, providers
  - `Nurse` - Nursing staff
  - `Receptionist` - Front desk
  - `Pharmacist` - Pharmacy staff
  - `Patient` - Patients
  - `SystemService` - Service-to-service calls

**Usage:**
```csharp
// In Program.cs - JWT setup
var jwtSettings = configuration.GetSection("Jwt").Get<JwtSettings>();
services.AddSingleton(jwtSettings);
services.AddScoped<JwtTokenProvider>();

// JWT authentication
services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSettings.SecretKey))
        };
    });

// Authorization policies
services.AddAuthorization(options =>
{
    AuthorizationPolicies.RegisterPolicies(options);
});

// In Login endpoint
var tokenProvider = serviceProvider.GetRequiredService<JwtTokenProvider>();
var accessToken = tokenProvider.GenerateAccessToken(
    userId: user.Id.ToString(),
    userName: user.UserName,
    email: user.Email,
    roles: user.Roles.ToList()
);
var refreshToken = tokenProvider.GenerateRefreshToken(
    userId: user.Id.ToString(),
    userName: user.UserName,
    email: user.Email
);

// In controllers
[Authorize(Policy = AuthorizationPolicies.ClinicianOrAdmin)]
[HttpPost("create-prescription")]
public async Task<IActionResult> CreatePrescription(CreatePrescriptionRequest request)
{
    var userId = _currentUser.UserId; // From ICurrentUserService
    var isAdmin = _currentUser.HasRole("Admin");
    // ...
}

// Password hashing
var hashedPassword = EncryptionService.HashPassword("UserPassword123!");
var isValid = EncryptionService.VerifyPassword("UserPassword123!", hashedPassword);

// Encryption
var encrypted = EncryptionService.EncryptAes256(sensitiveData, encryptionKey);
var decrypted = EncryptionService.DecryptAes256(encrypted, encryptionKey);
```

---

### 5. **Observability** - Monitoring, Logging, Metrics

Health checks, structured logging, application metrics, correlation ID tracking.

**Key Classes:**
- **Health Checks:**
  - `PostgresHealthCheck` - Database connectivity
  - `MongoHealthCheck` - MongoDB connectivity
  - `RedisHealthCheck` - Cache connectivity
  - `RabbitMqHealthCheck` - Message broker connectivity
- `IStructuredLogger` - Consistent JSON logging with audit trail
- `ApplicationMetrics` - Custom business metrics (counters)
- `CorrelationIdMiddleware` - Unique request ID propagation

**Usage:**
```csharp
// In Program.cs - Health checks
services.AddHealthChecks()
    .AddCheck<PostgresHealthCheck>("postgres")
    .AddCheck<MongoHealthCheck>("mongodb")
    .AddCheck<RedisHealthCheck>("redis")
    .AddCheck<RabbitMqHealthCheck>("rabbitmq");

// Map health check endpoint
app.MapHealthChecks("/health");

// Correlation ID middleware (early in pipeline)
app.UseCorrelationId();

// In controllers/services - Structured logging
[HttpPost]
public async Task<IActionResult> CreatePatient(CreatePatientRequest request)
{
    var correlationId = HttpContext.GetCorrelationId();
    
    _logger.LogInformation("Creating patient", new Dictionary<string, object>
    {
        ["firstName"] = request.FirstName,
        ["lastName"] = request.LastName,
    }, correlationId);
    
    // ...
    
    // Audit logging
    _logger.LogAudit(
        action: "CREATE_PATIENT",
        resource: $"Patient/{patient.Id}",
        userId: _currentUser.UserId!,
        success: true,
        traceId: correlationId
    );
}

// Application metrics
[HttpPost]
public async Task<IActionResult> CreatePatient(CreatePatientRequest request)
{
    var patient = new Patient { /* ... */ };
    _metrics.RecordPatientCreated(patient.Id.ToString());
    return Ok(patient);
}
```

---

### 6. **EventBus** - Asynchronous Event-Driven Communication

Domain events, integration events, outbox pattern for reliable event publishing.

**Key Classes & Events:**
- `DomainEvent` - Internal events within a service (MediatR/INotification)
- `IntegrationEvent` - Cross-service events via message broker
- **Integration Events (15 total):**
  - Patient: `PatientCreatedIntegrationEvent`, `PatientUpdatedIntegrationEvent`, `PatientDeletedIntegrationEvent`
  - Appointment: `AppointmentScheduledIntegrationEvent`, `AppointmentConfirmedIntegrationEvent`, `AppointmentCancelledIntegrationEvent`, `AppointmentCompletedIntegrationEvent`
  - Clinical: `ClinicalNoteCreatedIntegrationEvent`, `DiagnosisRecordedIntegrationEvent`, `PrescriptionCreatedIntegrationEvent`
  - Billing: `InvoiceGeneratedIntegrationEvent`, `PaymentProcessedIntegrationEvent`
  - Notification: `NotificationSentIntegrationEvent`
  - Integration: `HL7MessageReceivedIntegrationEvent`, `FhirResourceSyncedIntegrationEvent`
- `OutboxMessage` - Reliable event persistence (transactional outbox pattern)
- `OutboxProcessor` - Background service for guaranteed event delivery

**OUTBOX PATTERN Flow:**
```
1. Service writes business data + OutboxMessage in SAME transaction
   ↓
2. Transaction commits → both succeed or both fail
   ↓
3. OutboxProcessor reads unpublished messages (periodically, 5-second intervals)
   ↓
4. Publishes to RabbitMQ/Kafka
   ↓
5. Marks as published in database
   ↓
6. Consumer services process event (idempotently)
```

**Usage:**
```csharp
// In Patient Service - Create patient with event
public async Task<Patient> CreatePatientAsync(CreatePatientRequest request, string userId)
{
    var patient = new Patient
    {
        FirstName = request.FirstName,
        LastName = request.LastName,
        Email = request.Email,
        CreatedBy = userId
    };
    
    // Publish domain event (internal to service)
    patient.AddDomainEvent(new PatientCreatedDomainEvent
    {
        AggregateId = patient.Id,
        FirstName = patient.FirstName,
        LastName = patient.LastName
    });
    
    await _dbContext.Patients.AddAsync(patient);
    
    // Add outbox message for integration (cross-service)
    var outboxMessage = new OutboxMessage
    {
        AggregateId = patient.Id,
        AggregateType = "Patient",
        EventType = nameof(PatientCreatedIntegrationEvent),
        EventData = JsonSerializer.Serialize(new PatientCreatedIntegrationEvent
        {
            PatientId = patient.Id,
            FirstName = patient.FirstName,
            LastName = patient.LastName,
            Email = patient.Email,
            DateOfBirth = patient.DateOfBirth,
            CorrelationId = HttpContext.GetCorrelationId(),
            UserId = userId
        }),
        CorrelationId = HttpContext.GetCorrelationId()
    };
    
    await _dbContext.OutboxMessages.AddAsync(outboxMessage);
    await _dbContext.SaveChangesAsync(); // Single transaction
    
    return patient;
}

// In Notification Service - Handle integration event
public class PatientCreatedIntegrationEventHandler :
    IIntegrationEventHandler<PatientCreatedIntegrationEvent>
{
    private readonly INotificationService _notificationService;
    
    public async Task Handle(PatientCreatedIntegrationEvent @event, CancellationToken cancellationToken)
    {
        // Send welcome notification
        await _notificationService.SendWelcomeEmailAsync(
            email: @event.Email,
            name: @event.FirstName,
            cancellationToken: cancellationToken
        );
        
        // Log audit trail
        _logger.LogAudit(
            action: "PATIENT_REGISTERED",
            resource: $"Patient/{@event.PatientId}",
            userId: @event.UserId ?? "System",
            success: true
        );
    }
}

// OutboxProcessor implementation (in each service)
public class PatientServiceOutboxProcessor : OutboxProcessor
{
    private readonly IServiceProvider _serviceProvider;
    
    public PatientServiceOutboxProcessor(
        ILogger logger,
        IServiceProvider serviceProvider
    ) : base(logger)
    {
        _serviceProvider = serviceProvider;
    }
    
    protected override async Task<List<OutboxMessage>> GetUnpublishedMessagesAsync(
        CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<PatientDbContext>();
        return await context.OutboxMessages
            .Where(m => !m.IsPublished && m.PublishAttempts < m.MaxPublishAttempts)
            .OrderBy(m => m.CreatedAt)
            .Take(100)
            .ToListAsync(cancellationToken);
    }
    
    protected override async Task PublishMessageAsync(
        OutboxMessage message,
        CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateAsyncScope();
        var publisher = scope.ServiceProvider.GetRequiredService<IMessagePublisher>();
        
        var eventData = JsonSerializer.Deserialize<object>(message.EventData)!;
        await publisher.PublishAsync(message.EventType, eventData, cancellationToken);
    }
    
    protected override async Task MarkAsPublishedAsync(
        Guid messageId,
        CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<PatientDbContext>();
        
        var message = await context.OutboxMessages.FindAsync(new object[] { messageId }, cancellationToken);
        if (message != null)
        {
            message.MarkAsPublished();
            await context.SaveChangesAsync(cancellationToken);
        }
    }
    
    protected override async Task UpdateFailedAttemptAsync(
        Guid messageId,
        string error,
        CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<PatientDbContext>();
        
        var message = await context.OutboxMessages.FindAsync(new object[] { messageId }, cancellationToken);
        if (message != null)
        {
            message.RecordFailedAttempt(error);
            await context.SaveChangesAsync(cancellationToken);
        }
    }
}
```

---

## 🏗️ Architecture Patterns Used

### 1. **Domain-Driven Design (DDD)**
- Entities with business logic, value objects, specifications
- Ubiquitous language: entities, aggregates, domain events
- Repository pattern for data access

### 2. **Clean Architecture**
- Separation of concerns: Contracts, Common, Security, Observability
- Dependency inversion: Interfaces for all services
- No business logic in infrastructure layer

### 3. **Functional Error Handling**
- `Result<T>` pattern instead of exceptions for expected errors
- Composable error handling with Map/FlatMap
- Immutable error messages for audit trails

### 4. **Event-Driven Architecture**
- Domain events for intra-service state changes
- Integration events for inter-service communication
- Outbox pattern for guaranteed delivery (at-least-once)
- Idempotent event handlers

### 5. **Observability as First-Class**
- Correlation IDs for distributed tracing
- Structured JSON logging for easy parsing
- Health checks for all external dependencies
- OpenTelemetry integration for Prometheus/Grafana

---

## 🔐 HIPAA Compliance

Building-blocks follow HIPAA standards:

✅ **Authentication**
- JWT tokens with configurable expiration
- Bcrypt password hashing (work factor 12)
- Multi-factor authentication support

✅ **Encryption**
- AES-256-GCM for sensitive data at rest
- TLS/HTTPS for data in transit
- Per-service encryption keys

✅ **Audit Trail**
- Structured logging of all entity changes
- CorrelationId for request tracing
- 7-year audit log retention (MongoDB TTL)

✅ **Authorization**
- Role-based access control (RBAC) with 7 roles
- Policy-based authorization
- Fine-grained permissions per endpoint

✅ **Data Protection**
- Soft deletes (no permanent data loss)
- PII masking in logs
- Separation of duties (clinician, receptionist, patient, admin)

---

## 📚 Usage in Microservices

### In Program.cs
```csharp
// Add all building-block services
services.AddCommonExtensions();
services.AddSecurityServices(Configuration);
services.AddObservabilityServices(Configuration);
services.AddEventBusServices(Configuration);

// Add health checks
services.AddHealthChecks()
    .AddPostgresHealthCheck(Configuration["Database:ConnectionString"])
    .AddMongoHealthCheck(Configuration["Mongo:ConnectionString"]);

// Add authorization
services.AddAuthorization(options =>
    AuthorizationPolicies.RegisterPolicies(options));

// Middleware
app.UseCorrelationId();
app.UseAuthentication();
app.UseAuthorization();
app.MapHealthChecks("/health");

// Background services
services.AddHostedService<PatientServiceOutboxProcessor>();
```

### In Controllers
```csharp
[ApiController]
[Route("api/v1/[controller]")]
public class PatientsController : ControllerBase
{
    private readonly IPatientService _patientService;
    private readonly ICurrentUserService _currentUser;
    private readonly IStructuredLogger _logger;
    
    [Authorize(Policy = AuthorizationPolicies.ClinicianOrAdmin)]
    [HttpPost]
    public async Task<ApiResponse<PatientDto>> CreatePatient(
        [FromBody] CreatePatientRequest request)
    {
        var result = await _patientService.CreateAsync(request, _currentUser.UserId!);
        _logger.LogInformation("Patient created", new Dictionary<string, object>
        {
            ["patientId"] = result.Id,
            ["createdBy"] = _currentUser.UserId
        });
        
        return ApiResponse<PatientDto>.Created(
            _mapper.Map<PatientDto>(result),
            traceId: HttpContext.GetCorrelationId()
        );
    }
}
```

---

## 🧪 Testing

Mock implementations for unit testing:
```csharp
var mockCurrentUser = new MockCurrentUserService
{
    UserId = "test-user-123",
    UserName = "Dr. Smith",
    Email = "smith@hospital.com",
    Roles = new[] { "Clinician" },
    IsAuthenticated = true
};

// Use in test
var service = new PatientService(mockCurrentUser, mockRepository);
```

---

## 📦 NuGet Dependencies

All packages use minimal, well-maintained dependencies:

- **SecurityTokens**: `System.IdentityModel.Tokens.Jwt`
- **Password Hashing**: `BCrypt.Net-Core`
- **Database**: `Npgsql` (PostgreSQL), `MongoDB.Driver`
- **Messaging**: `RabbitMQ.Client`
- **Logging**: `Serilog` (optional, for JSON output)
- **Serialization**: `System.Text.Json`
- **Caching**: `StackExchange.Redis`
- **CQRS**: `MediatR`
- **AutoMapper**: For DTO mapping

---

## 🚀 Next Steps

1. **Reference in Services**: Each microservice adds NuGet references to needed building-blocks
2. **Implement Interfaces**: Services implement `IOutboxProcessor`, `IMessagePublisher`, etc.
3. **Extend Specifications**: Create service-specific `Specification<T>` subclasses
4. **Custom Events**: Extend `IntegrationEvent` for domain-specific events
5. **Register Services**: Call extension methods in Program.cs

---

## 📖 References

- Domain-Driven Design: Eric Evans
- Microservices Patterns: Chris Richardson
- HIPAA Compliance: https://www.hhs.gov/hipaa/
- OpenTelemetry: https://opentelemetry.io/
- MediatR: https://github.com/jbogard/MediatR
