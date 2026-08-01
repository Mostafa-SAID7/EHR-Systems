using EHRPlatform.BuildingBlocks.Common.Application.CQRS;

namespace EHRPlatform.Services.Appointment.Features.Appointments.Commands;

/// <summary>
/// Complete appointment command.
/// </summary>
public record CompleteAppointmentCommand : ICommand
{
    public Guid AppointmentId { get; init; }
}


