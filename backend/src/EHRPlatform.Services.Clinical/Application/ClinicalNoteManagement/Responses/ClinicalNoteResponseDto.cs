namespace EHRPlatform.Services.Clinical.Application.ClinicalNoteManagement.Responses;

/// <summary>
/// Clinical note response DTO.
/// Single Responsibility: Represent clinical note in API responses.
/// </summary>
public class ClinicalNoteResponseDto
{
    public Guid Id { get; set; }
    public Guid PatientId { get; set; }
    public Guid ProviderId { get; set; }
    public DateTime EncounterDate { get; set; }
    public string EncounterType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Subjective { get; set; } = string.Empty;
    public string Objective { get; set; } = string.Empty;
    public string Assessment { get; set; } = string.Empty;
    public string Plan { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? LastModifiedAt { get; set; }
    public List<VitalSignsDto> VitalSigns { get; set; } = new();
    public List<DiagnosisDto> Diagnoses { get; set; } = new();
    public List<ProcedureDto> Procedures { get; set; } = new();
}
