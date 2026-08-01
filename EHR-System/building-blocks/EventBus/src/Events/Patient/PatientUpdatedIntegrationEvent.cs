using System;

namespace EHRPlatform.EventBus.Events;

/// <summary>
/// Published when patient information is updated.
/// Single responsibility: Patient update event.
/// </summary>
public class PatientUpdatedIntegrationEvent : IntegrationEvent
{
    public Guid PatientId { get; set; }
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
}
