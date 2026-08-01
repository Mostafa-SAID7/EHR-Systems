namespace EHRPlatform.Services.Integration.Application.Services;

/// <summary>
/// Interface for FHIR resource validation service.
/// Validates FHIR resources against R4 profiles.
/// </summary>
public interface IFHIRValidationService
{
    /// <summary>
    /// Validates FHIR resource JSON content.
    /// </summary>
    Task<FHIRValidationResult> ValidateFHIRAsync(string fhirContent, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates FHIR resource against specific profile.
    /// </summary>
    Task<FHIRValidationResult> ValidateAgainstProfileAsync(
        string fhirContent,
        string profileUrl,
        CancellationToken cancellationToken = default);
}

public class FHIRValidationResult
{
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
    public List<string> Information { get; set; } = new();
}
