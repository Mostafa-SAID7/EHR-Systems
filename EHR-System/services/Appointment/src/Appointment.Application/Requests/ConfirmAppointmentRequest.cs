namespace EHRPlatform.Services.Appointment.Application.Appointments.Requests;

/// <summary>
/// Confirm appointment request DTO.
/// Contains data required to confirm an appointment.
/// </summary>
public class ConfirmAppointmentRequest
{
    /// <summary>Gets or sets the appointment identifier to confirm.</summary>
    public Guid AppointmentId { get; set; }
}
