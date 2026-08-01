using EHRPlatform.BuildingBlocks.SharedKernel.Entities;

namespace EHRPlatform.Services.Clinical.Domain.Entities;

/// <summary>
/// Clinical procedure performed.
/// </summary>
public class ClinicalProcedure : BaseEntity
{
    public Guid ClinicalNoteId { get; set; }
    public string ProcedureName { get; set; } = string.Empty;
    public string ProcedureCode { get; set; } = string.Empty; // CPT or SNOMED code
    public DateTime PerformedAt { get; set; }
    public string Result { get; set; } = string.Empty;
    public ClinicalNote ClinicalNote { get; set; } = null!;
}
