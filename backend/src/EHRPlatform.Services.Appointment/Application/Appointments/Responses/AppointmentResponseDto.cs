using EHRPlatform.Common.DTOs;

namespace EHRPlatform.Services.Appointment.Application.Appointments.Responses;

/// <summary>
/// Appointment response DTO.
/// Contains appointment details for API responses.
/// </summary>
public class AppointmentResponseDto : StatusDto
{
    /// <summary>Gets or sets the patient identifier.</summary>
    public Guid PatientId { get; set; }

    /// <summary>Gets or sets the provider identifier.</summary>
    public Guid ProviderId { get; set; }

    /// <summary>Gets or sets the scheduled start time.</summary>
    public DateTime ScheduledStart { get; set; }

    /// <summary>Gets or sets the scheduled end time.</summary>
    public DateTime ScheduledEnd { get; set; }

    /// <summary>Gets or sets the appointment type (Office, Telehealth, Phone).</summary>
    public string AppointmentType { get; set; } = string.Empty;

    /// <summary>Gets or sets the URL-friendly slug for appointment type.</summary>
    public string? AppointmentTypeSlug { get; set; }

    /// <summary>Gets or sets the reason for visit.</summary>
    public string? ReasonForVisit { get; set; }

    /// <summary>Gets or sets additional notes about the appointment.</summary>
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
}
