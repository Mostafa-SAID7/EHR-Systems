using EHRPlatform.Common.CQRS;

namespace EHRPlatform.Services.Appointment.Features.Appointments.Commands;

/// <summary>
/// Add note to appointment command.
/// </summary>
public record AddNoteCommand : ICommand
{
    public Guid AppointmentId { get; init; }
    public string Content { get; init; } = string.Empty;
    public Guid CreatedById { get; init; }
    public string? PrivacyLevel { get; init; } = "InternalOnly";
    public string? Category { get; init; }
}
