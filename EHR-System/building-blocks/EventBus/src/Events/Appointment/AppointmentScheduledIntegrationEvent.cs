using System;

namespace EHRPlatform.EventBus.Events;

/// <summary>
/// Published when an appointment is scheduled.
/// Consumed by: Notification (send confirmation), Analytics, Audit.
/// Single responsibility: Appointment scheduling event.
/// </summary>
public class AppointmentScheduledIntegrationEvent : IntegrationEvent
{
    public Guid AppointmentId { get; set; }
    public Guid PatientId { get; set; }
    public Guid ProviderId { get; set; }
    public DateTime AppointmentDate { get; set; }
    public string AppointmentType { get; set; } = null!;
}
