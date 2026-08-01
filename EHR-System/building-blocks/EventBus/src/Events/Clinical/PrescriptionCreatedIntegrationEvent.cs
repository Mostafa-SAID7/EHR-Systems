using System;

namespace EHRPlatform.EventBus.Events;

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
