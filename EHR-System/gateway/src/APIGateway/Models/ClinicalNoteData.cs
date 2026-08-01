namespace EHRPlatform.Gateway.Models;

/// <summary>
/// Clinical note data from Clinical Service.
/// </summary>
public class ClinicalNoteData
{
    public string Id { get; set; } = string.Empty;
    public string PatientId { get; set; } = string.Empty;
    public string ProviderId { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string NoteType { get; set; } = string.Empty;
}
