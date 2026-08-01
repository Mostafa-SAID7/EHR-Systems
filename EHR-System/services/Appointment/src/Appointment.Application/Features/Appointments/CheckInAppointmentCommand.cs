using EHRPlatform.BuildingBlocks.Common.Application.CQRS;

namespace EHRPlatform.Services.Appointment.Features.Appointments.Commands;

/// <summary>
/// Check-in appointment command.
/// </summary>
public record CheckInAppointmentCommand : ICommand
{
    public Guid AppointmentId { get; init; }
}


