using EHRPlatform.Common.Application.CQRS;

namespace EHRPlatform.Services.Prescription.Features.Prescriptions.Commands;

/// <summary>
/// Approve refill command.
/// </summary>
public record ApproveRefillCommand : ICommand
{
    public Guid PrescriptionId { get; init; }
    public Guid RefillId { get; init; }
}

