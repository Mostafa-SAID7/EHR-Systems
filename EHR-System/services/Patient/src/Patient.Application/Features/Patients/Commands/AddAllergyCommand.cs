using EHRPlatform.BuildingBlocks.Common.Application.CQRS;

namespace EHRPlatform.Services.Patient.Application.Features.Patients.Commands;

/// <summary>
/// Add allergy command.
/// Adds a new allergy to patient's medical history.
/// </summary>
public record AddAllergyCommand : ICommand
{
    public Guid PatientId { get; init; }
    public string Allergen { get; init; } = string.Empty;
    public string Severity { get; init; } = string.Empty; // Mild, Moderate, Severe
    public string Notes { get; init; } = string.Empty;
}



