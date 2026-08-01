using EHRPlatform.BuildingBlocks.Common.Events;

namespace EHRPlatform.Services.Appointment.Domain.Events;

/// <summary>
/// Domain event raised when an appointment is rescheduled.
/// </summary>
public class AppointmentRescheduledEvent : IntegrationEvent
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
    /// Gets the original scheduled start time.
    /// </summary>
    public DateTime OriginalStart { get; set; }

    /// <summary>
    /// Gets the new scheduled start time.
    /// </summary>
    public DateTime NewStart { get; set; }

    /// <summary>
    /// Gets the new scheduled end time.
    /// </summary>
    public DateTime NewEnd { get; set; }

    /// <summary>
    /// Gets the reason for rescheduling.
    /// </summary>
    public string? Reason { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="AppointmentRescheduledEvent"/> class.
    /// </summary>
    public AppointmentRescheduledEvent(
        Guid appointmentId,
        Guid patientId,
        Guid providerId,
        DateTime originalStart,
        DateTime newStart,
        DateTime newEnd,
        string? reason)
    {
        AppointmentId = appointmentId;
        PatientId = patientId;
        ProviderId = providerId;
        OriginalStart = originalStart;
        NewStart = newStart;
        NewEnd = newEnd;
        Reason = reason;
    }
}

