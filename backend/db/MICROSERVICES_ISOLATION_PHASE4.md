# Phase 4: Define Service Contracts (DTOs)

## Overview

Service contracts (DTOs and Events) define how microservices communicate. These contracts are the **ONLY** way services exchange data - NOT through shared domain entities.

## Architecture Pattern

```
┌─────────────────────────────────────────────────────────────┐
│ EHRPlatform.Common/Shared/DTOs/                            │
│ ===============================================            │
│ These are CONTRACTS between services                       │
│ Published once, consumed by multiple services              │
│                                                             │
│ ✅ UserDto            (Identity Service publishes)         │
│ ✅ PatientDto         (Patient Service publishes)          │
│ ✅ ClinicalDto        (Clinical Service publishes)         │
│ ✅ AppointmentDto     (Appointment Service publishes)      │
│ ✅ BillingDto         (Billing Service publishes)          │
│ ✅ PrescriptionDto    (Prescription Service publishes)     │
│ ✅ NotificationDto    (Notification Service publishes)     │
│ ✅ AuditDto           (Audit Service publishes)            │
│ ✅ AnalyticsDto       (Analytics Service publishes)        │
└─────────────────────────────────────────────────────────────┘
                              ↓↑
                    Kafka/MassTransit
                         (Events)
                              ↓↑
┌────────────────────────────────────────────────────────────┐
│ Service-Specific Implementation                           │
│ ================================================          │
│                                                            │
│ Identity Service:      Uses UserDto events               │
│ Patient Service:       Uses PatientDto events            │
│ Clinical Service:      Uses ClinicalDto events           │
│ Appointment Service:   Uses AppointmentDto events        │
│ Billing Service:       Uses BillingDto, AppointmentDto   │
│ Prescription Service:  Uses PrescriptionDto events       │
│ Notification Service:  Consumes ALL events               │
│ Audit Service:         Logs ALL events                   │
│ Analytics Service:     Analyzes ALL events               │
└────────────────────────────────────────────────────────────┘
```

## Contract-First Design

All DTOs follow this principle:
1. **DTO** = Data Transfer Object (value representation, no logic)
2. **Event** = DTO + metadata (what happened, when, who triggered it)
3. **Immutable** = Cannot be modified once created
4. **Versioned** = Can evolve with backward compatibility

---

## Phase 4: Complete DTO Mapping

### User Events (Identity Service)

**Service:** Identity Service  
**Publishes:**
- `UserCreatedEvent` → Notification Service (create preferences), Audit
- `UserUpdatedEvent` → All services (invalidate cache)
- `UserRoleAssignedEvent` → All services (recalculate permissions)
- `UserDeactivatedEvent` → All services (revoke access)

**Files:** `src/EHRPlatform.Common/Shared/DTOs/UserDto.cs`

### Patient Events (Patient Service)

**Service:** Patient Service  
**Publishes:**
- `PatientCreatedEvent` → Appointment, Clinical, Billing services
- `PatientUpdatedEvent` → Clinical, Notification services
- `PatientArchivedEvent` → All services (stop processing)
- `PatientAllergyAddedEvent` → Clinical, Prescription services
- `PatientConditionAddedEvent` → Clinical, Billing services
- `PatientStatusChangedEvent` → All services

**Files:** `src/EHRPlatform.Common/Shared/DTOs/PatientDto.cs`

### Clinical Events (Clinical Service)

**Service:** Clinical Service  
**Publishes:**
- `ClinicalNoteCreatedEvent` → Audit, Analytics services
- `VitalSignsRecordedEvent` → Analytics, Notification services (alerts)
- `DiagnosisCreatedEvent` → Patient, Billing services

**Files:** `src/EHRPlatform.Common/Shared/DTOs/ClinicalDto.cs`

### Appointment Events (Appointment Service)

**Service:** Appointment Service  
**Publishes:**
- `AppointmentScheduledEvent` → Notification, Analytics, Audit services
- `AppointmentConfirmedEvent` → Notification, Clinical services
- `AppointmentCancelledEvent` → Notification, Clinical, Billing services
- `AppointmentCompletedEvent` → Billing, Clinical, Analytics services
- `AppointmentRescheduledEvent` → Notification service

**Files:** `src/EHRPlatform.Common/Shared/DTOs/AppointmentDto.cs`

### Billing Events (Billing Service)

**Service:** Billing Service  
**Publishes:**
- `InvoiceGeneratedEvent` → Notification, Audit, Analytics services
- `PaymentReceivedEvent` → Notification, Audit, Analytics, Patient services
- `InvoiceOverdueEvent` → Notification, Audit services
- `PaymentFailedEvent` → Notification, Audit services

**Files:** `src/EHRPlatform.Common/Shared/DTOs/BillingDto.cs`

### Prescription Events (Prescription Service)

**Service:** Prescription Service  
**Publishes:**
- `PrescriptionCreatedEvent` → Notification, Audit, Analytics services
- `PrescriptionFilledEvent` → Notification, Patient services
- `PrescriptionRefillRequestedEvent` → Notification, Audit services
- `PrescriptionRefillApprovedEvent` → Notification, Audit services
- `PrescriptionCancelledEvent` → Notification, Audit services

**Files:** `src/EHRPlatform.Common/Shared/DTOs/PrescriptionDto.cs`

### Notification Events (Notification Service)

**Service:** Notification Service  
**Publishes:**
- `EmailNotificationSentEvent` → Audit service
- `SmsNotificationSentEvent` → Audit service
- `NotificationFailedEvent` → Audit service

**Files:** `src/EHRPlatform.Common/Shared/DTOs/NotificationDto.cs`

### Audit Events (Audit Service)

**Service:** Audit Service  
**Publishes:**
- `DataAccessLoggedEvent` → Compliance systems
- `DataModificationLoggedEvent` → Compliance systems
- `SecurityIncidentLoggedEvent` → Security monitoring

**Files:** `src/EHRPlatform.Common/Shared/DTOs/AuditDto.cs`

### Analytics Events (Analytics Service)

**Service:** Analytics Service  
**Publishes:**
- `ReportGeneratedEvent` → Admin dashboard, Compliance
- `MetricsAggregatedEvent` → Real-time dashboard, Monitoring
- `AnalyticsAlertGeneratedEvent` → Management, Compliance
- `ScheduledReportExportedEvent` → Email delivery, Audit

**Files:** `src/EHRPlatform.Common/Shared/DTOs/AnalyticsDto.cs`

---

## Event-Driven Communication Patterns

### Pattern 1: Request-Reply via Events

```csharp
// Clinical Service needs to create a billing record after appointment
public async Task HandleAppointmentCompletedAsync(AppointmentCompletedEvent @event)
{
    // Map DTO to internal entity
    var appointment = MapperService.ToAppointmentEntity(@event);
    
    // Process locally
    var clinicalNote = await clinicalService.GenerateNoteAsync(appointment);
    
    // Publish back
    var @clinicalNoteEvent = new ClinicalNoteCreatedEvent
    {
        ClinicalNoteId = clinicalNote.Id,
        PatientId = appointment.PatientId,
        // ...
    };
    
    await eventBus.PublishAsync(clinicalNoteEvent);
    // Billing Service listens to ClinicalNoteCreatedEvent (via AppointmentCompletedEvent)
}
```

### Pattern 2: Fanout Publishing

```csharp
// One event triggers actions in multiple services
public async Task HandlePatientCreatedAsync(PatientCreatedEvent @event)
{
    // Appointment Service: Create appointment slots
    await appointmentService.CreateSlotsAsync(@event.PatientId);
    
    // Clinical Service: Initialize medical record
    await clinicalService.InitializeMedicalRecordAsync(@event.PatientId);
    
    // Billing Service: Setup billing account
    await billingService.SetupAccountAsync(@event.PatientId, @event.PatientData);
    
    // Notification Service: Send welcome email
    await notificationService.SendWelcomeEmailAsync(@event.PatientData);
}
```

### Pattern 3: Event Enrichment

```csharp
// Service receives minimal event, enriches it with local data
public async Task HandleUserRoleAssignedAsync(UserRoleAssignedEvent @event)
{
    // Get full user details (cache or API)
    var user = await userService.GetUserAsync(@event.UserId);
    
    // Enrich the event
    var enrichedEvent = new UserRoleAssignedEnrichedEvent
    {
        UserId = @event.UserId,
        RoleName = @event.RoleName,
        UserEmail = user.Email,      // Enriched data
        UserName = user.FirstName,   // Enriched data
        OccurredAt = @event.OccurredAt
    };
    
    // Use enriched data for downstream processing
    await cache.InvalidateUserPermissionsAsync(@event.UserId);
}
```

---

## DTO Best Practices

### ✅ DO: Keep DTOs Simple

```csharp
// ✅ GOOD: Simple, focused DTO
public class PatientDto
{
    public Guid Id { get; set; }
    public string MedicalRecordNumber { get; set; }
    public string FirstName { get; set; }
    public string Email { get; set; }
    public DateTime DateOfBirth { get; set; }
}

// ❌ WRONG: Too much data, tightly coupled
public class PatientDto
{
    public Guid Id { get; set; }
    public PatientContactDto PrimaryContact { get; set; }
    public List<PatientAllergyDto> Allergies { get; set; }
    public List<PatientConditionDto> Conditions { get; set; }
    public PatientMedicalHistoryDto MedicalHistory { get; set; }
    public List<AppointmentDto> UpcomingAppointments { get; set; }
    // ... too much!
}
```

### ✅ DO: Version Events for Breaking Changes

```csharp
// Original event
public class PatientCreatedEvent
{
    public Guid PatientId { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
}

// Breaking change: add new required field
// Create new event class instead of modifying
[Obsolete("Use PatientCreatedEventV2")]
public class PatientCreatedEvent { }

public class PatientCreatedEventV2
{
    public Guid PatientId { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public DateTime DateOfBirth { get; set; }  // NEW required field
}

// Publishers use V2, consumers handle both during migration
```

### ✅ DO: Include Context in Events

```csharp
// ✅ GOOD: Event includes context
public class PatientArchivedEvent
{
    public Guid PatientId { get; set; }
    public string Reason { get; set; }              // Why archived?
    public Guid ArchivedByUserId { get; set; }     // Who did it?
    public DateTime OccurredAt { get; set; }        // When?
}
```

### ❌ DON'T: Share Internal DTOs

```csharp
// ❌ WRONG: Exposing internal implementation
namespace EHRPlatform.Services.Patient.Application.DTOs
{
    public class InternalPatientUpdateDto { }
}

// ❌ WRONG: Other services importing from service-specific namespace
using EHRPlatform.Services.Patient.Application.DTOs;

// ✅ RIGHT: Only share from Common/Shared/DTOs
using EHRPlatform.Common.Shared.DTOs;
```

---

## Event Serialization and Versioning

### Using Newtonsoft.Json

```csharp
// In Program.cs
var settings = new JsonSerializerSettings
{
    TypeNameHandling = TypeNameHandling.Auto,
    NullValueHandling = NullValueHandling.Ignore,
    DateFormatString = "yyyy-MM-ddTHH:mm:ss.fffZ"
};

builder.Services.AddSingleton(settings);
```

### Handling Event Versioning

```csharp
// In event handler
public async Task HandlePatientEventAsync(JObject eventData)
{
    // Detect version from event
    var eventType = eventData["$type"]?.ToString();
    
    if (eventType.Contains("PatientCreatedEventV2"))
    {
        var @event = eventData.ToObject<PatientCreatedEventV2>();
        // Handle V2
    }
    else if (eventType.Contains("PatientCreatedEvent"))
    {
        var @event = eventData.ToObject<PatientCreatedEvent>();
        // Handle V1, adapt to V2 logic
    }
}
```

---

## Event Consumer Examples

### Example 1: Notification Service (Consumes All Events)

```csharp
// Notification Service subscribes to multiple event types
public class NotificationEventHandler
{
    private readonly INotificationService _service;
    private readonly ILogger<NotificationEventHandler> _logger;

    public NotificationEventHandler(INotificationService service, 
        ILogger<NotificationEventHandler> logger)
    {
        _service = service;
        _logger = logger;
    }

    // Handle Patient Created
    public async Task HandlePatientCreatedAsync(PatientCreatedEvent @event)
    {
        _logger.LogInformation($"New patient: {event.PatientId}");
        await _service.SendWelcomeEmailAsync(@event.PatientData.Email, 
            @event.PatientData.FirstName);
    }

    // Handle Appointment Scheduled
    public async Task HandleAppointmentScheduledAsync(AppointmentScheduledEvent @event)
    {
        _logger.LogInformation($"Appointment: {event.AppointmentId}");
        await _service.SendAppointmentConfirmationAsync(@event);
    }

    // Handle Invoice Generated
    public async Task HandleInvoiceGeneratedAsync(InvoiceGeneratedEvent @event)
    {
        _logger.LogInformation($"Invoice: {event.InvoiceId}");
        await _service.SendInvoiceEmailAsync(@event);
    }
}
```

### Example 2: Audit Service (Logs Everything)

```csharp
// Audit Service subscribes to all events
public class AuditEventHandler
{
    private readonly IAuditService _auditService;

    public async Task<bool> OnBeforePublishAsync(string eventType, object eventData)
    {
        // Audit every published event
        var auditEntry = new AuditEntry
        {
            Action = "PUBLISH",
            ResourceType = eventType,
            NewValues = JsonConvert.SerializeObject(eventData),
            Timestamp = DateTime.UtcNow
        };

        await _auditService.LogAsync(auditEntry);
        return true; // Allow event to be published
    }
}
```

---

## Service-to-Service Communication Matrix

| Publisher | Event | Consumers |
|-----------|-------|-----------|
| Identity | UserCreatedEvent | Notification, Audit |
| Identity | UserRoleAssignedEvent | All Services |
| Patient | PatientCreatedEvent | Appointment, Clinical, Billing, Audit |
| Patient | PatientUpdatedEvent | Clinical, Notification, Audit |
| Patient | PatientAllergyAddedEvent | Clinical, Prescription, Audit |
| Clinical | ClinicalNoteCreatedEvent | Audit, Analytics |
| Clinical | DiagnosisCreatedEvent | Patient, Billing, Audit |
| Clinical | VitalSignsRecordedEvent | Analytics, Notification (alerts) |
| Appointment | AppointmentScheduledEvent | Notification, Audit, Analytics |
| Appointment | AppointmentCompletedEvent | Billing, Clinical, Audit, Analytics |
| Appointment | AppointmentCancelledEvent | Billing, Clinical, Notification, Audit |
| Billing | InvoiceGeneratedEvent | Notification, Audit, Analytics |
| Billing | PaymentReceivedEvent | Patient, Notification, Audit, Analytics |
| Prescription | PrescriptionCreatedEvent | Notification, Audit, Analytics |
| Prescription | PrescriptionFilledEvent | Patient, Notification, Audit |
| Notification | NotificationSentEvent | Audit |
| Audit | DataModificationLoggedEvent | Compliance |

---

## Testing DTOs

### Unit Test Pattern

```csharp
[Fact]
public void PatientCreatedEvent_Serialization_Roundtrip()
{
    // Arrange
    var @event = new PatientCreatedEvent
    {
        PatientId = Guid.NewGuid(),
        PatientData = new PatientDto
        {
            Id = Guid.NewGuid(),
            MedicalRecordNumber = "MRN001",
            FirstName = "John",
            LastName = "Doe",
            DateOfBirth = new DateTime(1990, 1, 1),
            Email = "john@example.com"
        },
        CreatedByUserId = Guid.NewGuid(),
        OccurredAt = DateTime.UtcNow
    };

    // Act
    var json = JsonConvert.SerializeObject(@event);
    var deserialized = JsonConvert.DeserializeObject<PatientCreatedEvent>(json);

    // Assert
    Assert.Equal(@event.PatientId, deserialized.PatientId);
    Assert.Equal(@event.PatientData.Email, deserialized.PatientData.Email);
}
```

---

## Verification Checklist

After implementing Phase 4, verify:

- [ ] All 9 service DTOs created in `EHRPlatform.Common/Shared/DTOs/`
- [ ] Each DTO is simple and focused
- [ ] Each service has multiple event types
- [ ] Events include context (who, when, why)
- [ ] No service-specific code in shared DTOs
- [ ] Events are immutable (only properties, no methods)
- [ ] Serialization/deserialization tested
- [ ] Event versioning strategy defined
- [ ] Communication matrix documented
- [ ] All consumers identified for each event

---

## Files Created (Phase 4)

✅ `src/EHRPlatform.Common/Shared/DTOs/UserDto.cs`  
✅ `src/EHRPlatform.Common/Shared/DTOs/PatientDto.cs`  
✅ `src/EHRPlatform.Common/Shared/DTOs/ClinicalDto.cs`  
✅ `src/EHRPlatform.Common/Shared/DTOs/AppointmentDto.cs`  
✅ `src/EHRPlatform.Common/Shared/DTOs/BillingDto.cs`  
✅ `src/EHRPlatform.Common/Shared/DTOs/PrescriptionDto.cs`  
✅ `src/EHRPlatform.Common/Shared/DTOs/NotificationDto.cs`  
✅ `src/EHRPlatform.Common/Shared/DTOs/AuditDto.cs`  
✅ `src/EHRPlatform.Common/Shared/DTOs/AnalyticsDto.cs`  

---

## Summary

**Phase 4 achieves:**
✅ All 9 services have defined contracts (DTOs + Events)  
✅ Clear communication patterns established  
✅ Service-to-service communication is event-driven  
✅ No direct entity sharing between services  
✅ Event versioning strategy in place  
✅ Immutable, simple DTOs  

**Result:**
- Services communicate via standardized contracts
- Breaking changes are managed through versioning
- New services can be added by subscribing to relevant events
- Full auditability of all inter-service communication

---

**Phase 4 Status:** Ready for Implementation  
**Estimated Duration:** Already complete (DTOs created)  
**Next:** Phase 5 - Verify Docker Compose configuration

