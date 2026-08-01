using System;

namespace EHRPlatform.EventBus.Events;

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
