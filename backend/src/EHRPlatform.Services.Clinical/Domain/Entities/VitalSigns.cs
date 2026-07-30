using EHRPlatform.Common.Domain.Entities;

namespace EHRPlatform.Services.Clinical.Domain.Entities;

/// <summary>
/// Vital signs measurement.
/// </summary>
public class VitalSigns : BaseEntity
{
    public Guid ClinicalNoteId { get; set; }
    public DateTime RecordedAt { get; set; }
    public decimal Temperature { get; set; } // Celsius
    public int SystolicBP { get; set; }
    public int DiastolicBP { get; set; }
    public int HeartRate { get; set; }
    public int RespiratoryRate { get; set; }
    public decimal? Weight { get; set; } // kg
    public ClinicalNote ClinicalNote { get; set; } = null!;

    public string GetBloodPressure() => $"{SystolicBP}/{DiastolicBP}";
}

