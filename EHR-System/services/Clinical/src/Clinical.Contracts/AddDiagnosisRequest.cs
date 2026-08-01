namespace EHRPlatform.Services.Clinical.Contracts;

/// <summary>
/// Request to add a diagnosis to a clinical note.
/// </summary>
public class AddDiagnosisRequest
{
    public string DiagnosisCode { get; set; } = string.Empty; // ICD-10 code
    public string DiagnosisText { get; set; } = string.Empty;
    public string DiagnosisType { get; set; } = "Secondary"; // Principal or Secondary
}
