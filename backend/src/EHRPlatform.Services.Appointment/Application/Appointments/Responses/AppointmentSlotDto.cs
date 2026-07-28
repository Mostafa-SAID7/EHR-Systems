namespace EHRPlatform.Services.Appointment.Application.Appointments.Responses;

/// <summary>
/// Appointment slot DTO.
/// Represents an appointment slot in a provider's calendar.
/// </summary>
public class AppointmentSlotDto
{
    /// <summary>Gets or sets the appointment identifier.</summary>
    public Guid AppointmentId { get; set; }

    /// <summary>Gets or sets the patient identifier.</summary>
    public Guid? PatientId { get; set; }

    /// <summary>Gets or sets the slot start time.</summary>
    public DateTime Start { get; set; }

    /// <summary>Gets or sets the slot end time.</summary>
    public DateTime End { get; set; }

    /// <summary>Gets or sets the appointment type.</summary>
    public string? AppointmentType { get; set; }

    /// <summary>Gets or sets the slot status (Available, Booked, Blocked).</summary>
    public string Status { get; set; } = string.Empty;
}
