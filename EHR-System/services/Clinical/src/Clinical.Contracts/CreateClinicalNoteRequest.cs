namespace EHRPlatform.Services.Clinical.Contracts;

/// <summary>
/// Request to create a new clinical note.
/// </summary>
public class CreateClinicalNoteRequest
{
    public Guid PatientId { get; set; }
    public Guid ProviderId { get; set; }
    public DateTime EncounterDate { get; set; }
    public string EncounterType { get; set; } = string.Empty; // Office, Telehealth, Emergency, Hospital
    public string? Subjective { get; set; }
    public string? Objective { get; set; }
    public string? Assessment { get; set; }
    public string? Plan { get; set; }
}
