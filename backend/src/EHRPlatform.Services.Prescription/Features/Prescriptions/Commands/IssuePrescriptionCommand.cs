using EHRPlatform.BuildingBlocks.Common.Application.CQRS;
using EHRPlatform.Services.Prescription.Application.PrescriptionManagement.Responses;

namespace EHRPlatform.Services.Prescription.Features.Prescriptions.Commands;

/// <summary>
/// Issue prescription command.
/// </summary>
public record IssuePrescriptionCommand : ICommand<PrescriptionResponseDto>
{
    public Guid PatientId { get; init; }
    public Guid ProviderId { get; init; }
    public string MedicationName { get; init; } = string.Empty;
    public string Strength { get; init; } = string.Empty;
    public string FormType { get; init; } = string.Empty;
    public string Dosage { get; init; } = string.Empty;
    public string Frequency { get; init; } = string.Empty;
    public int Quantity { get; init; }
    public int RefillsAllowed { get; init; }
    public DateTime StartDate { get; init; }
    public DateTime? EndDate { get; init; }
    public string? Indications { get; init; }
    public string? SpecialInstructions { get; init; }
    public bool IsControlledSubstance { get; init; }
    public string? NDCCode { get; init; }
}


