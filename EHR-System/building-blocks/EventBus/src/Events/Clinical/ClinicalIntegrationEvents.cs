using System;

namespace EHRPlatform.EventBus.Events;

/// <summary>
/// Published when a clinical note is created.
/// Consumed by: Audit, FileStorage (link documents), Analytics.
/// Single responsibility: Clinical note creation event.
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
/// Single responsibility: Diagnosis recording event.
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
/// Single responsibility: Prescription creation event.
/// </summary>
public class PrescriptionCreatedIntegrationEvent : IntegrationEvent
{
    public Guid PrescriptionId { get; set; }
    public Guid PatientId { get; set; }
    public Guid ProviderId { get; set; }
    public string MedicationName { get; set; } = null!;
    public string Dosage { get; set; } = null!;
}
