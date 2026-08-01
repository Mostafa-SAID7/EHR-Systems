namespace EHRPlatform.Services.Clinical.Contracts.Responses;

/// <summary>
/// Response DTO for vital signs.
/// </summary>
public class VitalSignsResponse
{
    public Guid Id { get; set; }
    public Guid ClinicalNoteId { get; set; }
    public DateTime RecordedAt { get; set; }
    public decimal Temperature { get; set; } // Celsius
    public int SystolicBP { get; set; }
    public int DiastolicBP { get; set; }
    public int HeartRate { get; set; }
    public int RespiratoryRate { get; set; }
    public decimal? Weight { get; set; } // kg

    public string GetBloodPressure() => $"{SystolicBP}/{DiastolicBP}";
}
