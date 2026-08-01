namespace EHRPlatform.Services.Appointment.Domain.Enums;

/// <summary>
/// Reminder notification status.
/// </summary>
public enum ReminderStatus
{
    /// <summary>Reminder scheduled but not sent yet.</summary>
    Scheduled = 1,

    /// <summary>Reminder successfully sent.</summary>
    Sent = 2,

    /// <summary>Reminder failed to send.</summary>
    Failed = 3,

    /// <summary>Reminder cancelled/skipped.</summary>
    Cancelled = 4
}
