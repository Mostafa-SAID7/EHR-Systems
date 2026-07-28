namespace EHRPlatform.Services.Appointment.Application.Appointments.Requests;

/// <summary>
/// Complete appointment request DTO.
/// Contains data required to mark an appointment as completed.
/// </summary>
public class CompleteAppointmentRequest
{
    /// <summary>Gets or sets the appointment identifier to complete.</summary>
    public Guid AppointmentId { get; set; }
}
