using EHRPlatform.BuildingBlocks.Common.Application.CQRS;

namespace EHRPlatform.Services.Appointment.Features.Appointments.Commands;

/// <summary>
/// Reschedule appointment command.
/// </summary>
public record RescheduleAppointmentCommand : ICommand
{
    public Guid AppointmentId { get; init; }
    public DateTime NewScheduledStart { get; init; }
    public int DurationMinutes { get; init; }
    public Guid InitiatedById { get; init; }
    public string InitiatedBy { get; init; } = "Provider";
    public string? Reason { get; init; }
}


