using EHRPlatform.BuildingBlocks.Common.Application.CQRS;

namespace EHRPlatform.Services.Appointment.Features.Appointments.Commands;

/// <summary>
/// Cancel appointment command.
/// </summary>
public record CancelAppointmentCommand : ICommand
{
    public Guid AppointmentId { get; init; }
    public string Reason { get; init; } = string.Empty;
}


