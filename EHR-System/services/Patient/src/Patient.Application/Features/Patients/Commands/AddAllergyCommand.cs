namespace EHRPlatform.Services.Patient.Application.Features.Patients.Commands;

using MediatR;

/// <summary>
/// Command to add allergy to patient record.
/// </summary>
public class AddAllergyCommand : IRequest<AddAllergyResponse>
{
    public Guid PatientId { get; set; }
    public string AllergyCode { get; set; } = string.Empty; // SNOMED CT
    public string AllergyName { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty; // Mild, Moderate, Severe
    public string? ReactionDescription { get; set; }
}

public class AddAllergyResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public Guid? AllergyId { get; set; }
}
