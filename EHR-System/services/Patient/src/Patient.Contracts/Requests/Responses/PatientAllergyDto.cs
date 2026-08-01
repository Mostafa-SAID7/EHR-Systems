namespace EHRPlatform.Services.Patient.Application.Patients.Responses;

/// <summary>
/// Patient allergy nested DTO.
/// </summary>
public class PatientAllergyDto
{
    public Guid Id { get; set; }
    public string Allergen { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty; // Mild, Moderate, Severe
    public string Notes { get; set; } = string.Empty;
}
