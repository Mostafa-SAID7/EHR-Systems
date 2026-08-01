namespace EHRPlatform.Services.Clinical.Contracts.Responses;

/// <summary>
/// Response DTO for a clinical diagnosis.
/// </summary>
public class DiagnosisResponse
{
    public Guid Id { get; set; }
    public Guid ClinicalNoteId { get; set; }
    public string DiagnosisCode { get; set; } = string.Empty; // ICD-10
    public string DiagnosisText { get; set; } = string.Empty;
    public string DiagnosisType { get; set; } = string.Empty; // Principal, Secondary
}
