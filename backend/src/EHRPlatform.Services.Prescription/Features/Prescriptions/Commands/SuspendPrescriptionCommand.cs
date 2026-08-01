using EHRPlatform.BuildingBlocks.Common.Application.CQRS;

namespace EHRPlatform.Services.Prescription.Features.Prescriptions.Commands;

/// <summary>
/// Suspend prescription command.
/// </summary>
public record SuspendPrescriptionCommand : ICommand
{
    public Guid PrescriptionId { get; init; }
    public string Reason { get; init; } = string.Empty;
}


