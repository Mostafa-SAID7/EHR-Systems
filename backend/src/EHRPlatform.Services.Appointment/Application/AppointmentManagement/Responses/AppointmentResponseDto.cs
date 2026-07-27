using EHRPlatform.Common.DTOs;

namespace EHRPlatform.Services.Appointment.Application.AppointmentManagement.Responses;

/// <summary>
/// Appointment response DTO with slug support for AppointmentType.
/// Includes URL-friendly slug for appointment type-based routing.
/// </summary>
public class AppointmentResponseDto : StatusDto
{
    public Guid PatientId { get; set; }
    public Guid ProviderId { get; set; }
    public DateTime ScheduledStart { get; set; }
    public DateTime ScheduledEnd { get; set; }
    
    /// <summary>
    /// Type of appointment (Office, Telehealth, Phone).
    /// </summary>
    public string AppointmentType { get; set; } = string.Empty;
    
    /// <summary>
    /// URL-friendly slug for AppointmentType (e.g., "office", "telehealth", "phone").
    /// </summary>
    public string? AppointmentTypeSlug { get; set; }
    
    public string? ReasonForVisit { get; set; }
    public string? Notes { get; set; }
    public int DurationMinutes { get; set; }
    public bool ReminderSent { get; set; }
    public DateTime? ConfirmedAt { get; set; }
    public DateTime? CancelledAt { get; set; }
    public string? CancellationReason { get; set; }
}

