namespace EHRPlatform.Services.Appointment.Application.Appointments.Requests;

/// <summary>
/// Cancel appointment request DTO.
/// Contains data required to cancel an appointment.
/// </summary>
public class CancelAppointmentRequest
{
    /// <summary>Gets or sets the appointment identifier to cancel.</summary>
    public Guid AppointmentId { get; set; }

    /// <summary>Gets or sets the cancellation reason.</summary>
    public string Reason { get; set; } = string.Empty;
}
