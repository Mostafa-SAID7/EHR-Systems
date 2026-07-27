namespace EHRPlatform.Services.Clinical.Application.ClinicalNoteManagement.Responses;

/// <summary>
/// Common diagnosis DTO.
/// </summary>
public class DiagnosisDto
{
    public Guid Id { get; set; }
    public string DiagnosisCode { get; set; } = string.Empty;
    public string DiagnosisText { get; set; } = string.Empty;
    public string DiagnosisType { get; set; } = string.Empty;
}

/// <summary>
/// Vital signs DTO.
/// </summary>
public class VitalSignsDto
{
    public Guid Id { get; set; }
    public DateTime RecordedAt { get; set; }
    public decimal Temperature { get; set; }
    public int SystolicBP { get; set; }
    public int DiastolicBP { get; set; }
    public int HeartRate { get; set; }
    public int RespiratoryRate { get; set; }
    public decimal? Weight { get; set; }
}

/// <summary>
/// Procedure DTO.
/// </summary>
public class ProcedureDto
{
    public Guid Id { get; set; }
    public string ProcedureCode { get; set; } = string.Empty;
    public string ProcedureName { get; set; } = string.Empty;
    public DateTime PerformedDate { get; set; }
}

/// <summary>
/// Clinical note timeline item DTO.
/// Used for paged timeline results.
/// </summary>
public class ClinicalNoteTimelineItemDto
{
    public Guid Id { get; set; }
    public DateTime EncounterDate { get; set; }
    public string EncounterType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public Guid ProviderId { get; set; }
    public List<DiagnosisDto> Diagnoses { get; set; } = new();
    public VitalSignsDto? LatestVitals { get; set; }
}
