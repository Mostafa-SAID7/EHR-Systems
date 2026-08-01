using EHRPlatform.BuildingBlocks.Common.Events;

namespace EHRPlatform.Services.Appointment.Domain.Events;

/// <summary>
/// Domain event raised when an appointment is completed.
/// </summary>
public class AppointmentCompletedEvent : IntegrationEvent
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
    /// Gets the completion time.
    /// </summary>
    public DateTime CompletedAt { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="AppointmentCompletedEvent"/> class.
    /// </summary>
    /// <param name="id">The appointment identifier.</param>
    /// <param name="patientId">The patient identifier.</param>
    /// <param name="providerId">The provider identifier.</param>
    /// <param name="completed">The completion time.</param>
    public AppointmentCompletedEvent(Guid id, Guid patientId, Guid providerId, DateTime completed)
    {
        AppointmentId = id;
        PatientId = patientId;
        ProviderId = providerId;
        CompletedAt = completed;
    }
}

