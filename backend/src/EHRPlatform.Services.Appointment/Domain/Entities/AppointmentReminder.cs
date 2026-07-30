using EHRPlatform.Common.Domain.Entities;
using EHRPlatform.Services.Appointment.Domain.Enums;

namespace EHRPlatform.Services.Appointment.Features.Appointments.Domain;

/// <summary>
/// Appointment reminder notification entity.
/// Tracks reminder notifications sent for appointments.
/// </summary>
public class AppointmentReminder : BaseEntity
{
    /// <summary>
    /// Gets or sets the appointment identifier this reminder belongs to.
    /// </summary>
    public Guid AppointmentId { get; set; }

    /// <summary>
    /// Gets or sets the scheduled time for the reminder.
    /// </summary>
    public DateTime ReminderTime { get; set; }

    /// <summary>
    /// Gets or sets the reminder method (Email, SMS, InApp, Push).
    /// </summary>
    public ReminderType Method { get; set; }

    /// <summary>
    /// Gets or sets the reminder status (Scheduled, Sent, Failed, Cancelled).
    /// </summary>
    public ReminderStatus Status { get; set; } = ReminderStatus.Scheduled;

    /// <summary>
    /// Gets or sets a value indicating whether the reminder has been sent.
    /// </summary>
    public bool IsSent { get; set; }

    /// <summary>
    /// Gets or sets the date and time the reminder was actually sent.
    /// </summary>
    public DateTime? SentAt { get; set; }

    /// <summary>
    /// Gets or sets the parent appointment.
    /// </summary>
    public Appointment Appointment { get; set; } = null!;
}

