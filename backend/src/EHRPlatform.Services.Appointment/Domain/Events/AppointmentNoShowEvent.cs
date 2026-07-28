using EHRPlatform.Common.Events;

namespace EHRPlatform.Services.Appointment.Domain.Events;

/// <summary>
/// Domain event raised when an appointment is marked as no-show.
/// </summary>
public class AppointmentNoShowEvent : IntegrationEvent
{
    /// <summary>
    /// Gets the appointment identifier.
    /// </summary>
    public Guid AppointmentId { get; set; }

    /// <summary>
    /// Gets the patient identifier.
    /// </summary>
    public Guid PatientId { get; set; }

    /// <summary>
    /// Gets the provider identifier.
    /// </summary>
    public Guid ProviderId { get; set; }

    /// <summary>
    /// Gets the scheduled start time.
    /// </summary>
    public DateTime ScheduledStart { get; set; }

    /// <summary>
    /// Gets the time the no-show was recorded.
    /// </summary>
    public DateTime RecordedAt { get; set; }

    /// <summary>
    /// Gets the reason for no-show (if recorded).
    /// </summary>
    public string? Reason { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="AppointmentNoShowEvent"/> class.
    /// </summary>
    public AppointmentNoShowEvent(
        Guid appointmentId,
        Guid patientId,
        Guid providerId,
        DateTime scheduledStart,
        DateTime recordedAt,
        string? reason = null)
    {
        AppointmentId = appointmentId;
        PatientId = patientId;
        ProviderId = providerId;
        ScheduledStart = scheduledStart;
        RecordedAt = recordedAt;
        Reason = reason;
    }
}
