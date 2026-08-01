namespace EHRPlatform.Services.Appointment.Application.Features.Appointments.Commands;

using MediatR;

/// <summary>
/// Command to schedule new appointment with conflict detection.
/// </summary>
public class ScheduleAppointmentCommand : IRequest<ScheduleAppointmentResponse>
{
    public Guid PatientId { get; set; }
    public Guid ProviderId { get; set; }
    public DateTime ScheduledStart { get; set; }
    public DateTime ScheduledEnd { get; set; }
    public string AppointmentType { get; set; } = "Office"; // Office, Telehealth, Phone
    public string ReasonForVisit { get; set; } = string.Empty;
    public List<AppointmentReminderDto> Reminders { get; set; } = new(); // Email at 15min, SMS at 1day before
}

public class AppointmentReminderDto
{
    public string Method { get; set; } = string.Empty; // Email, SMS, Push, InApp
    public int MinutesBefore { get; set; }
}

public class ScheduleAppointmentResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public Guid? AppointmentId { get; set; }
}
