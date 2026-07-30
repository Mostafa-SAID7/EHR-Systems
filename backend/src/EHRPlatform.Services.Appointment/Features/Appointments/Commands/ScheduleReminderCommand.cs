using EHRPlatform.Common.Application.CQRS;
using EHRPlatform.Services.Appointment.Domain.Enums;

namespace EHRPlatform.Services.Appointment.Features.Appointments.Commands;

/// <summary>
/// Schedule appointment reminder command.
/// </summary>
public record ScheduleReminderCommand : ICommand
{
    public Guid AppointmentId { get; init; }
    public DateTime ReminderTime { get; init; }
    public ReminderType ReminderType { get; init; }
}

