namespace EHRPlatform.Services.Clinical.Contracts.Responses;

/// <summary>
/// Response DTO for a clinical note.
/// </summary>
public class ClinicalNoteResponse
{
    public Guid Id { get; set; }
    public Guid PatientId { get; set; }
    public Guid ProviderId { get; set; }
    public DateTime EncounterDate { get; set; }
    public string EncounterType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    
    // SOAP components
    public string Subjective { get; set; } = string.Empty;
    public string Objective { get; set; } = string.Empty;
    public string Assessment { get; set; } = string.Empty;
    public string Plan { get; set; } = string.Empty;
    
    // Related data
    public List<VitalSignsResponse> VitalSigns { get; set; } = new();
    public List<DiagnosisResponse> Diagnoses { get; set; } = new();
    public List<ProcedureResponse> Procedures { get; set; } = new();
    
    // Audit trail
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
}
