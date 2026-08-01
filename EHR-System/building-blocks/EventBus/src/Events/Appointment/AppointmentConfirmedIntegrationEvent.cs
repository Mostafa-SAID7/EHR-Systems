using System;

namespace EHRPlatform.EventBus.Events;

/// <summary>
/// Published when an appointment is confirmed by provider.
/// Single responsibility: Appointment confirmation event.
/// </summary>
public class AppointmentConfirmedIntegrationEvent : IntegrationEvent
{
    public Guid AppointmentId { get; set; }
    public Guid PatientId { get; set; }
    public DateTime ConfirmedAt { get; set; }
}
