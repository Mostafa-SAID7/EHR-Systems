using EHRPlatform.BuildingBlocks.Common.Events;

namespace EHRPlatform.Services.Appointment.Domain.Events;

/// <summary>
/// Domain event raised when an appointment is cancelled.
/// </summary>
public class AppointmentCancelledEvent : IntegrationEvent
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
    /// Gets the cancellation reason.
    /// </summary>
    public string Reason { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="AppointmentCancelledEvent"/> class.
    /// </summary>
    /// <param name="id">The appointment identifier.</param>
    /// <param name="patientId">The patient identifier.</param>
    /// <param name="providerId">The provider identifier.</param>
    /// <param name="reason">The cancellation reason.</param>
    public AppointmentCancelledEvent(Guid id, Guid patientId, Guid providerId, string reason)
    {
        AppointmentId = id;
        PatientId = patientId;
        ProviderId = providerId;
        Reason = reason;
    }
}

