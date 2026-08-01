using EHRPlatform.BuildingBlocks.Common.Events;

namespace EHRPlatform.Services.Appointment.Domain.Events;

/// <summary>
/// Domain event raised when an appointment is scheduled.
/// </summary>
public class AppointmentScheduledEvent : IntegrationEvent
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
    /// Gets the appointment type.
    /// </summary>
    public string AppointmentType { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="AppointmentScheduledEvent"/> class.
    /// </summary>
    /// <param name="id">The appointment identifier.</param>
    /// <param name="patientId">The patient identifier.</param>
    /// <param name="providerId">The provider identifier.</param>
    /// <param name="start">The scheduled start time.</param>
    /// <param name="type">The appointment type.</param>
    public AppointmentScheduledEvent(Guid id, Guid patientId, Guid providerId, DateTime start, string type)
    {
        AppointmentId = id;
        PatientId = patientId;
        ProviderId = providerId;
        ScheduledStart = start;
        AppointmentType = type;
    }
}

