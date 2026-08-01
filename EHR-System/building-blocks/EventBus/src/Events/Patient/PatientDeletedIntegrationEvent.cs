using System;

namespace EHRPlatform.EventBus.Events;

/// <summary>
/// Published when a patient is deleted/archived.
/// Single responsibility: Patient deletion event.
/// </summary>
public class PatientDeletedIntegrationEvent : IntegrationEvent
{
    public Guid PatientId { get; set; }
    public string Reason { get; set; } = null!;
}
