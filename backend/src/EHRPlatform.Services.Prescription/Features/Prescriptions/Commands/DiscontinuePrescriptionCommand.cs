using EHRPlatform.BuildingBlocks.Common.Application.CQRS;

namespace EHRPlatform.Services.Prescription.Features.Prescriptions.Commands;

/// <summary>
/// Discontinue prescription command.
/// </summary>
public record DiscontinuePrescriptionCommand : ICommand
{
    public Guid PrescriptionId { get; init; }
    public string Reason { get; init; } = string.Empty;
}


