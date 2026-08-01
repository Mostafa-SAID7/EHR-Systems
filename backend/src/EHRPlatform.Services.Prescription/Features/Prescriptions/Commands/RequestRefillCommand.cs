using EHRPlatform.BuildingBlocks.Common.Application.CQRS;

namespace EHRPlatform.Services.Prescription.Features.Prescriptions.Commands;

/// <summary>
/// Request refill command.
/// </summary>
public record RequestRefillCommand : ICommand
{
    public Guid PrescriptionId { get; init; }
    public string? PharmacyId { get; init; }
}


