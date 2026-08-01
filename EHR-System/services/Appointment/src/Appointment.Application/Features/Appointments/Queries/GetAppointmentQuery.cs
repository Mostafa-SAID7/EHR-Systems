namespace EHRPlatform.Services.Appointment.Application.Features.Appointments.Queries;

using MediatR;

/// <summary>
/// Query to get appointment details.
/// </summary>
public class GetAppointmentQuery : IRequest<GetAppointmentResponse>
{
    public Guid AppointmentId { get; set; }
}

public class GetAppointmentResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public AppointmentDto? Appointment { get; set; }
}

public class AppointmentDto
{
    public Guid Id { get; set; }
    public Guid PatientId { get; set; }
    public Guid ProviderId { get; set; }
    public string AppointmentType { get; set; } = string.Empty;
    public DateTime ScheduledStart { get; set; }
    public DateTime ScheduledEnd { get; set; }
    public string Status { get; set; } = string.Empty;
    public string ReasonForVisit { get; set; } = string.Empty;
    public int DurationMinutes { get; set; }
    public DateTime CreatedAt { get; set; }
}
