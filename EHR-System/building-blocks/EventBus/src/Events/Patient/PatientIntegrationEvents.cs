using System;

namespace EHRPlatform.EventBus.Events;

/// <summary>
/// Published when a new patient is registered in the system.
/// Consumed by: Notification, Analytics, Audit services.
/// Single responsibility: Patient creation event.
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
/// Single responsibility: Patient update event.
/// </summary>
public class PatientUpdatedIntegrationEvent : IntegrationEvent
{
    public Guid PatientId { get; set; }
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
}

/// <summary>
/// Published when a patient is deleted/archived.
/// Single responsibility: Patient deletion event.
/// </summary>
public class PatientDeletedIntegrationEvent : IntegrationEvent
{
    public Guid PatientId { get; set; }
    public string Reason { get; set; } = null!;
}
