namespace EHRPlatform.Services.Appointment.Controllers.Requests;

/// <summary>
/// Request model for rescheduling an appointment.
/// </summary>
public class RescheduleAppointmentRequest
{
    /// <summary>Gets or sets the new scheduled start time.</summary>
    public DateTime NewScheduledStart { get; set; }

    /// <summary>Gets or sets the duration in minutes.</summary>
    public int DurationMinutes { get; set; }

    /// <summary>Gets or sets the user ID who initiated the reschedule.</summary>
    public Guid InitiatedById { get; set; }

    /// <summary>Gets or sets who initiated (Patient, Provider, Admin).</summary>
    public string? InitiatedBy { get; set; } = "Provider";

    /// <summary>Gets or sets the reason for rescheduling.</summary>
    public string? Reason { get; set; }
}
