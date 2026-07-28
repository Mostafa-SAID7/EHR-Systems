namespace EHRPlatform.Services.Appointment.Application.Appointments.Requests;

/// <summary>
/// Check-in appointment request DTO.
/// Contains data required to check in to an appointment.
/// </summary>
public class CheckInAppointmentRequest
{
    /// <summary>Gets or sets the appointment identifier to check in.</summary>
    public Guid AppointmentId { get; set; }
}
