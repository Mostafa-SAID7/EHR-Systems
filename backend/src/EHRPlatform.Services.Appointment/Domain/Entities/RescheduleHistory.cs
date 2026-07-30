using EHRPlatform.Common.Domain.Entities;

namespace EHRPlatform.Services.Appointment.Features.Appointments.Domain;

/// <summary>
/// Reschedule history entity.
/// Tracks all rescheduling actions for an appointment.
/// Provides audit trail for compliance and analytics.
/// </summary>
public class RescheduleHistory : BaseEntity
{
    /// <summary>
    /// Gets or sets the appointment identifier.
    /// </summary>
    public Guid AppointmentId { get; set; }

    /// <summary>
    /// Gets or sets the original scheduled start time.
    /// </summary>
    public DateTime OriginalScheduledStart { get; set; }

    /// <summary>
    /// Gets or sets the new scheduled start time after rescheduling.
    /// </summary>
    public DateTime NewScheduledStart { get; set; }

    /// <summary>
    /// Gets or sets who initiated the reschedule (Patient, Provider, Admin).
    /// </summary>
    public string InitiatedBy { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the user ID of who initiated the reschedule.
    /// </summary>
    public Guid InitiatedByUserId { get; set; }

    /// <summary>
    /// Gets or sets the reason for rescheduling.
    /// </summary>
    public string? Reason { get; set; }

    /// <summary>
    /// Gets or sets whether the reschedule was automatic (system-initiated).
    /// </summary>
    public bool IsAutomatic { get; set; }

    /// <summary>
    /// Gets or sets the date and time the reschedule occurred.
    /// </summary>
    public DateTime RescheduleDateTime { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the parent appointment.
    /// </summary>
    public Appointment Appointment { get; set; } = null!;
}

