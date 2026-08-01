namespace EHRPlatform.Services.Clinical.Contracts;

/// <summary>
/// Request to update SOAP components of a clinical note.
/// </summary>
public class UpdateSOAPRequest
{
    public string? Subjective { get; set; }
    public string? Objective { get; set; }
    public string? Assessment { get; set; }
    public string? Plan { get; set; }
}
