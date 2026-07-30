using EHRPlatform.Common.Application.CQRS;
using EHRPlatform.Services.Appointment.Application.AppointmentManagement.Responses;

namespace EHRPlatform.Services.Appointment.Features.Appointments.Commands;

/// <summary>
/// Schedule appointment command.
/// </summary>
public record ScheduleAppointmentCommand : ICommand<AppointmentResponseDto>
{
    public Guid PatientId { get; init; }
    public Guid ProviderId { get; init; }
    public DateTime ScheduledStart { get; init; }
    public int DurationMinutes { get; init; }
    public string AppointmentType { get; init; } = string.Empty;
    public string? ReasonForVisit { get; init; }
    public string? Notes { get; init; }
}


