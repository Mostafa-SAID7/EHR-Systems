namespace EHRPlatform.Services.Appointment.Application.Appointments.Responses;

/// <summary>
/// Appointment reminder DTO.
/// Contains reminder information for an appointment.
/// </summary>
public class AppointmentReminderDto
{
    /// <summary>Gets or sets the reminder identifier.</summary>
    public Guid Id { get; set; }

    /// <summary>Gets or sets the appointment identifier.</summary>
    public Guid AppointmentId { get; set; }

    /// <summary>Gets or sets the reminder date and time.</summary>
    public DateTime ReminderDateTime { get; set; }

    /// <summary>Gets or sets the delivery channel (Email, SMS, Push, InApp).</summary>
    public string Channel { get; set; } = string.Empty;

    /// <summary>Gets or sets the reminder status (Scheduled, Sent, Failed).</summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>Gets or sets the time the reminder was actually sent.</summary>
    public DateTime? SentAt { get; set; }
}
