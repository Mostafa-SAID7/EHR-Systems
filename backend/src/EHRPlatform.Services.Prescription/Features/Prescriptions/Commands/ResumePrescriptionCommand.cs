using EHRPlatform.BuildingBlocks.Common.Application.CQRS;

namespace EHRPlatform.Services.Prescription.Features.Prescriptions.Commands;

/// <summary>
/// Resume prescription command.
/// </summary>
public record ResumePrescriptionCommand : ICommand
{
    public Guid PrescriptionId { get; init; }
}


