using EHRPlatform.BuildingBlocks.Common.Application.CQRS;

namespace EHRPlatform.Services.Appointment.Features.Appointments.Commands;

/// <summary>
/// Send appointment reminder command.
/// </summary>
public record SendReminderCommand : ICommand
{
    public Guid ReminderId { get; init; }
}


