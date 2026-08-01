namespace EHRPlatform.Services.Clinical.Contracts;

/// <summary>
/// Request to record vital signs for a clinical note.
/// </summary>
public class RecordVitalsRequest
{
    public decimal Temperature { get; set; } // Celsius
    public int SystolicBP { get; set; }
    public int DiastolicBP { get; set; }
    public int HeartRate { get; set; }
    public int RespiratoryRate { get; set; }
    public decimal? Weight { get; set; } // kg
}
