using EHRPlatform.BuildingBlocks.SharedKernel.Entities;

namespace EHRPlatform.Services.Patient.Domain.Entities;

/// <summary>
/// Patient allergy record.
/// </summary>
public class PatientAllergy : BaseEntity
{
    public Guid PatientId { get; set; }
    public string Allergen { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty; // Mild, Moderate, Severe
    public string Notes { get; set; } = string.Empty;
    public Patient Patient { get; set; } = null!;
}


