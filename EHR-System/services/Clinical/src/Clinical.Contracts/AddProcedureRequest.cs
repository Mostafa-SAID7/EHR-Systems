namespace EHRPlatform.Services.Clinical.Contracts;

/// <summary>
/// Request to add a procedure to a clinical note.
/// </summary>
public class AddProcedureRequest
{
    public string ProcedureName { get; set; } = string.Empty;
    public string ProcedureCode { get; set; } = string.Empty; // CPT or SNOMED code
    public string Result { get; set; } = string.Empty;
}
