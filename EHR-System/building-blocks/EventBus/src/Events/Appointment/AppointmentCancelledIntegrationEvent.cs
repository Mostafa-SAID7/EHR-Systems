using System;

namespace EHRPlatform.EventBus.Events;

/// <summary>
/// Published when an appointment is cancelled.
/// Consumed by: Notification (notify patient), Analytics, Calendar cleanup.
/// Single responsibility: Appointment cancellation event.
/// </summary>
public class AppointmentCancelledIntegrationEvent : IntegrationEvent
{
    public Guid AppointmentId { get; set; }
    public Guid PatientId { get; set; }
    public string CancellationReason { get; set; } = null!;
}
