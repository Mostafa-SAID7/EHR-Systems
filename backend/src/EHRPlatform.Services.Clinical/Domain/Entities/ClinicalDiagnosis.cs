using EHRPlatform.Common.Domain.Entities;

namespace EHRPlatform.Services.Clinical.Domain.Entities;

/// <summary>
/// Clinical diagnosis (ICD-10).
/// </summary>
public class ClinicalDiagnosis : BaseEntity
{
    public Guid ClinicalNoteId { get; set; }
    public string DiagnosisCode { get; set; } = string.Empty; // ICD-10 code
    public string DiagnosisText { get; set; } = string.Empty;
    public string DiagnosisType { get; set; } = string.Empty; // Principal, Secondary
    public ClinicalNote ClinicalNote { get; set; } = null!;
}

