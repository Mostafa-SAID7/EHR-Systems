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
