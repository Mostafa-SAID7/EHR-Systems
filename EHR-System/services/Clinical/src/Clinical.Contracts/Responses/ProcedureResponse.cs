namespace EHRPlatform.Services.Clinical.Contracts.Responses;

/// <summary>
/// Response DTO for a clinical procedure.
/// </summary>
public class ProcedureResponse
{
    public Guid Id { get; set; }
    public Guid ClinicalNoteId { get; set; }
    public string ProcedureName { get; set; } = string.Empty;
    public string ProcedureCode { get; set; } = string.Empty; // CPT or SNOMED
    public DateTime PerformedAt { get; set; }
    public string Result { get; set; } = string.Empty;
}
