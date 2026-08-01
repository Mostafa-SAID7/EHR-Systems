namespace EHRPlatform.Services.Appointment.Application.Appointments.Responses;

/// <summary>
/// Detailed appointment response DTO.
/// Includes reminders and computed fields for detailed appointment views.
/// </summary>
public class AppointmentDetailedResponseDto
{
    /// <summary>Gets or sets the appointment identifier.</summary>
    public Guid Id { get; set; }

    /// <summary>Gets or sets the patient identifier.</summary>
    public Guid PatientId { get; set; }

    /// <summary>Gets or sets the provider identifier.</summary>
    public Guid ProviderId { get; set; }

    /// <summary>Gets or sets the scheduled start time.</summary>
    public DateTime ScheduledStart { get; set; }

    /// <summary>Gets or sets the scheduled end time.</summary>
    public DateTime ScheduledEnd { get; set; }

    /// <summary>
    /// Gets or sets the type of appointment (Office, Telehealth, Phone).
    /// </summary>
    public string AppointmentType { get; set; } = string.Empty;

    /// <summary>Gets or sets the current status.</summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>Gets or sets the reason for visit.</summary>
    public string? ReasonForVisit { get; set; }

    /// <summary>Gets or sets additional notes.</summary>
    public string? Notes { get; set; }

    /// <summary>Gets or sets the duration in minutes.</summary>
    public int DurationMinutes { get; set; }

    /// <summary>Gets or sets a value indicating whether a reminder has been sent.</summary>
    public bool ReminderSent { get; set; }

    /// <summary>Gets or sets the confirmation time.</summary>
    public DateTime? ConfirmedAt { get; set; }

    /// <summary>Gets or sets the cancellation time.</summary>
    public DateTime? CancelledAt { get; set; }

    /// <summary>Gets or sets the cancellation reason.</summary>
    public string? CancellationReason { get; set; }

    /// <summary>Gets or sets the list of reminders for this appointment.</summary>
    public List<AppointmentReminderDto> Reminders { get; set; } = new();

    /// <summary>Gets or sets a value indicating whether the appointment is available.</summary>
    public bool IsAvailable { get; set; }

    /// <summary>Gets or sets the time until appointment in minutes.</summary>
    public double TimeUntilAppointment { get; set; }
}
