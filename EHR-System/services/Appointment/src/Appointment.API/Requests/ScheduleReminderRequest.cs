namespace EHRPlatform.Services.Appointment.Controllers.Requests;

using EHRPlatform.Services.Appointment.Domain.Enums;

/// <summary>
/// Request model for scheduling a reminder.
/// </summary>
public class ScheduleReminderRequest
{
    /// <summary>Gets or sets the time to send the reminder.</summary>
    public DateTime ReminderTime { get; set; }

    /// <summary>Gets or sets the reminder type (Email, SMS, InApp, Push).</summary>
    public ReminderType ReminderType { get; set; } = ReminderType.Email;
}
