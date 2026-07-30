using EHRPlatform.Common.Application.CQRS;

namespace EHRPlatform.Services.Appointment.Features.Appointments.Commands;

/// <summary>
/// Confirm appointment command.
/// </summary>
public record ConfirmAppointmentCommand : ICommand
{
    public Guid AppointmentId { get; init; }
}

