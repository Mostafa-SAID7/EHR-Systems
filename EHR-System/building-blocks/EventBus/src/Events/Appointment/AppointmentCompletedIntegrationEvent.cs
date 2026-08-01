using System;

namespace EHRPlatform.EventBus.Events;

/// <summary>
/// Published when an appointment is completed.
/// Single responsibility: Appointment completion event.
/// </summary>
public class AppointmentCompletedIntegrationEvent : IntegrationEvent
{
    public Guid AppointmentId { get; set; }
    public Guid PatientId { get; set; }
    public int DurationMinutes { get; set; }
}
