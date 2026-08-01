using System;

namespace EHRPlatform.Common.Shared.DTOs
{
    /// <summary>
    /// Shared DTO for Clinical Note Communication
    /// Used for inter-service events (Clinical Service publishes, other services consume)
    /// </summary>
    public class ClinicalNoteDto
    {
        public Guid Id { get; set; }
        public Guid PatientId { get; set; }
        public Guid ProviderId { get; set; }
        public string ProviderName { get; set; }
        public string NoteType { get; set; }  // e.g., "Progress", "Discharge", "Consultation"
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    /// <summary>
    /// Shared DTO for Vital Signs Communication
    /// </summary>
    public class VitalSignsDto
    {
        public Guid Id { get; set; }
        public Guid PatientId { get; set; }
        public decimal Temperature { get; set; }  // Celsius
        public int SystolicBP { get; set; }       // Systolic Blood Pressure
        public int DiastolicBP { get; set; }      // Diastolic Blood Pressure
        public int HeartRate { get; set; }        // Beats per minute
        public int RespiratoryRate { get; set; }  // Breaths per minute
        public decimal? SpO2 { get; set; }        // Oxygen saturation percentage
        public DateTime MeasuredAt { get; set; }
    }

    /// <summary>
    /// Shared DTO for Diagnosis Communication
    /// </summary>
    public class DiagnosisDto
    {
        public Guid Id { get; set; }
        public Guid PatientId { get; set; }
        public string DiagnosisCode { get; set; }  // ICD-10 code
        public string DiagnosisName { get; set; }
        public string Severity { get; set; }       // e.g., "Mild", "Moderate", "Severe"
        public DateTime DiagnosedAt { get; set; }
    }

    /// <summary>
    /// Event: Clinical Note Created
    /// Published by Clinical Service when provider creates a note
    /// Subscribed by: Audit, Analytics services
    /// </summary>
    public class ClinicalNoteCreatedEvent
    {
        public Guid ClinicalNoteId { get; set; }
        public Guid PatientId { get; set; }
        public Guid ProviderId { get; set; }
        public string NoteType { get; set; }
        public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Event: Vital Signs Recorded
    /// Published by Clinical Service when vitals are measured
    /// Subscribed by: Analytics (trend analysis), Notification (alerts for abnormal values)
    /// </summary>
    public class VitalSignsRecordedEvent
    {
        public Guid VitalSignsId { get; set; }
        public Guid PatientId { get; set; }
        public decimal Temperature { get; set; }
        public int HeartRate { get; set; }
        public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Event: Diagnosis Created
    /// Published by Clinical Service when a diagnosis is recorded
    /// Subscribed by: Patient Service (update patient conditions), Billing (diagnosis-based billing)
    /// </summary>
    public class DiagnosisCreatedEvent
    {
        public Guid DiagnosisId { get; set; }
        public Guid PatientId { get; set; }
        public string DiagnosisCode { get; set; }
        public string DiagnosisName { get; set; }
        public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
    }
}
