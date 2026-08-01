using EHRPlatform.BuildingBlocks.Contracts.DTOs;

namespace EHRPlatform.Services.Appointment.Application.Appointments.Responses;

/// <summary>
/// Appointment command response DTO.
/// Used for command operation responses (Schedule, Confirm, Cancel, etc.).
/// </summary>
public class AppointmentCommandDto : StatusDto
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

    /// <summary>Gets or sets the appointment type (Office, Telehealth, Phone).</summary>
    public string AppointmentType { get; set; } = string.Empty;

    /// <summary>Gets or sets the reason for visit.</summary>
    public string? ReasonForVisit { get; set; }

    /// <summary>Gets or sets additional notes.</summary>
    public string? Notes { get; set; }

    /// <summary>Gets or sets the duration in minutes.</summary>
    public int DurationMinutes { get; set; }
}


