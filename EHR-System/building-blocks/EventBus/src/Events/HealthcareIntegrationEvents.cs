using System;

namespace EHRPlatform.EventBus.Events;

// ═══════════════════════════════════════════════════════════════════════════════
// PATIENT SERVICE INTEGRATION EVENTS
// ═══════════════════════════════════════════════════════════════════════════════

/// <summary>
/// Published when a new patient is registered in the system.
/// Consumed by: Notification, Analytics, Audit services.
/// </summary>
public class PatientCreatedIntegrationEvent : IntegrationEvent
{
    public Guid PatientId { get; set; }
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public DateTime DateOfBirth { get; set; }
}

/// <summary>
/// Published when patient information is updated.
/// </summary>
public class PatientUpdatedIntegrationEvent : IntegrationEvent
{
    public Guid PatientId { get; set; }
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
}

/// <summary>
/// Published when a patient is deleted/archived.
/// </summary>
public class PatientDeletedIntegrationEvent : IntegrationEvent
{
    public Guid PatientId { get; set; }
    public string Reason { get; set; } = null!;
}

// ═══════════════════════════════════════════════════════════════════════════════
// APPOINTMENT SERVICE INTEGRATION EVENTS
// ═══════════════════════════════════════════════════════════════════════════════

/// <summary>
/// Published when an appointment is scheduled.
/// Consumed by: Notification (send confirmation), Analytics, Audit.
/// </summary>
public class AppointmentScheduledIntegrationEvent : IntegrationEvent
{
    public Guid AppointmentId { get; set; }
    public Guid PatientId { get; set; }
    public Guid ProviderId { get; set; }
    public DateTime AppointmentDate { get; set; }
    public string AppointmentType { get; set; } = null!;
}

/// <summary>
/// Published when an appointment is confirmed by provider.
/// </summary>
public class AppointmentConfirmedIntegrationEvent : IntegrationEvent
{
    public Guid AppointmentId { get; set; }
    public Guid PatientId { get; set; }
    public DateTime ConfirmedAt { get; set; }
}

/// <summary>
/// Published when an appointment is cancelled.
/// Consumed by: Notification (notify patient), Analytics, Calendar cleanup.
/// </summary>
public class AppointmentCancelledIntegrationEvent : IntegrationEvent
{
    public Guid AppointmentId { get; set; }
    public Guid PatientId { get; set; }
    public string CancellationReason { get; set; } = null!;
}

/// <summary>
/// Published when an appointment is completed.
/// </summary>
public class AppointmentCompletedIntegrationEvent : IntegrationEvent
{
    public Guid AppointmentId { get; set; }
    public Guid PatientId { get; set; }
    public int DurationMinutes { get; set; }
}

// ═══════════════════════════════════════════════════════════════════════════════
// CLINICAL SERVICE INTEGRATION EVENTS
// ═══════════════════════════════════════════════════════════════════════════════

/// <summary>
/// Published when a clinical note is created.
/// Consumed by: Audit, FileStorage (link documents), Analytics.
/// </summary>
public class ClinicalNoteCreatedIntegrationEvent : IntegrationEvent
{
    public Guid NoteId { get; set; }
    public Guid PatientId { get; set; }
    public Guid ProviderId { get; set; }
    public string NoteType { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Published when a diagnosis is recorded.
/// Consumed by: Analytics, Terminology (code validation), Billing (coding).
/// </summary>
public class DiagnosisRecordedIntegrationEvent : IntegrationEvent
{
    public Guid DiagnosisId { get; set; }
    public Guid PatientId { get; set; }
    public string ICD10Code { get; set; } = null!;
    public string DiagnosisDescription { get; set; } = null!;
    public DateTime RecordedAt { get; set; }
}

/// <summary>
/// Published when a prescription is created.
/// Consumed by: Notification (notify patient), Pharmacy, Analytics.
/// </summary>
public class PrescriptionCreatedIntegrationEvent : IntegrationEvent
{
    public Guid PrescriptionId { get; set; }
    public Guid PatientId { get; set; }
    public Guid ProviderId { get; set; }
    public string MedicationName { get; set; } = null!;
    public string Dosage { get; set; } = null!;
}

// ═══════════════════════════════════════════════════════════════════════════════
// BILLING SERVICE INTEGRATION EVENTS
// ═══════════════════════════════════════════════════════════════════════════════

/// <summary>
/// Published when an invoice is generated.
/// Consumed by: Notification (send invoice), Analytics, Audit.
/// </summary>
public class InvoiceGeneratedIntegrationEvent : IntegrationEvent
{
    public Guid InvoiceId { get; set; }
    public Guid PatientId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
    public DateTime DueDate { get; set; }
}

/// <summary>
/// Published when payment is processed.
/// Consumed by: Notification (send receipt), Analytics, Accounting.
/// </summary>
public class PaymentProcessedIntegrationEvent : IntegrationEvent
{
    public Guid PaymentId { get; set; }
    public Guid InvoiceId { get; set; }
    public Guid PatientId { get; set; }
    public decimal Amount { get; set; }
    public string PaymentMethod { get; set; } = null!;
    public string Status { get; set; } = null!;
}

// ═══════════════════════════════════════════════════════════════════════════════
// NOTIFICATION SERVICE INTEGRATION EVENTS
// ═══════════════════════════════════════════════════════════════════════════════

/// <summary>
/// Published when a notification is sent.
/// Consumed by: Audit (compliance logging).
/// </summary>
public class NotificationSentIntegrationEvent : IntegrationEvent
{
    public Guid NotificationId { get; set; }
    public Guid RecipientId { get; set; }
    public string Channel { get; set; } = null!;
    public string MessageType { get; set; } = null!;
    public bool WasSuccessful { get; set; }
}

// ═══════════════════════════════════════════════════════════════════════════════
// INTEGRATION SERVICE INTEGRATION EVENTS
// ═══════════════════════════════════════════════════════════════════════════════

/// <summary>
/// Published when HL7 message is received and processed.
/// </summary>
public class HL7MessageReceivedIntegrationEvent : IntegrationEvent
{
    public Guid MessageId { get; set; }
    public Guid PatientId { get; set; }
    public string ExternalSystem { get; set; } = null!;
    public DateTime ReceivedAt { get; set; }
}

/// <summary>
/// Published when FHIR resource is synced from external system.
/// </summary>
public class FhirResourceSyncedIntegrationEvent : IntegrationEvent
{
    public Guid SyncId { get; set; }
    public Guid PatientId { get; set; }
    public string ResourceType { get; set; } = null!;
    public string ExternalId { get; set; } = null!;
}
