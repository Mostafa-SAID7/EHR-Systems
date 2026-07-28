namespace EHRPlatform.Services.Appointment.Application.Appointments.Requests;

/// <summary>
/// Schedule appointment request DTO.
/// Contains data required to schedule a new appointment.
/// </summary>
public class ScheduleAppointmentRequestDto
{
    /// <summary>Gets or sets the patient identifier.</summary>
    public Guid PatientId { get; set; }

    /// <summary>Gets or sets the provider identifier.</summary>
    public Guid ProviderId { get; set; }

    /// <summary>Gets or sets the scheduled start time.</summary>
    public DateTime ScheduledStart { get; set; }

    /// <summary>Gets or sets the appointment duration in minutes.</summary>
    public int DurationMinutes { get; set; }

    /// <summary>Gets or sets the appointment type (Office, Telehealth, Phone).</summary>
    public string AppointmentType { get; set; } = string.Empty;

    /// <summary>Gets or sets the reason for visit.</summary>
    public string? ReasonForVisit { get; set; }

    /// <summary>Gets or sets additional notes.</summary>
    public string? Notes { get; set; }
}
