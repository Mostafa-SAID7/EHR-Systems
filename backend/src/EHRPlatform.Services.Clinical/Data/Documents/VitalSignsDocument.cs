#nullable enable

using EHRPlatform.Common.Data;
using MongoDB.Bson.Serialization.Attributes;

namespace EHRPlatform.Services.Clinical.Data.Documents;

/// <summary>
/// MongoDB document for a single vital-signs measurement.
///
/// Design rationale — time-series in MongoDB:
///   VitalSigns are high-frequency, append-only measurements that benefit from
///   MongoDB's document model over PostgreSQL for several reasons:
///   - No fixed schema: future sensors can add fields (SpO2, ECG, glucose …)
///     without requiring a migration.
///   - Write throughput: MongoDB shards naturally on PatientId + RecordedAt.
///   - Aggregation pipeline: $group / $bucket / $avg run efficiently on BSON
///     date fields for trend analysis.
///
///   The PostgreSQL VitalSigns table is kept for FK integrity (ClinicalNoteId)
///   and billing/coding lookups.  This document adds the full sensor payload.
/// </summary>
public class VitalSignsDocument : MongoBaseDocument
{
    /// <summary>Links back to the PostgreSQL VitalSigns.Id.</summary>
    [BsonElement("vitalSignsId")]
    public Guid VitalSignsId { get; set; }

    /// <summary>Links back to the PostgreSQL ClinicalNote.Id.</summary>
    [BsonElement("clinicalNoteId")]
    public Guid ClinicalNoteId { get; set; }

    [BsonElement("patientId")]
    public Guid PatientId { get; set; }

    [BsonElement("recordedAt")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime RecordedAt { get; set; }

    // ── Core vitals (always present) ──────────────────────────────────────────

    [BsonElement("temperatureCelsius")]
    public decimal Temperature { get; set; }

    [BsonElement("systolicBP")]
    public int SystolicBP { get; set; }

    [BsonElement("diastolicBP")]
    public int DiastolicBP { get; set; }

    [BsonElement("heartRate")]
    public int HeartRate { get; set; }

    [BsonElement("respiratoryRate")]
    public int RespiratoryRate { get; set; }

    [BsonElement("weightKg")]
    public decimal? Weight { get; set; }

    // ── Extended vitals (sensor-driven, schema-flexible) ──────────────────────

    [BsonElement("spO2Percent")]
    public decimal? SpO2 { get; set; }

    [BsonElement("glucoseMgDl")]
    public decimal? GlucoseMgDl { get; set; }

    [BsonElement("painScale")]        // 0–10 NRS
    public int? PainScale { get; set; }

    [BsonElement("heightCm")]
    public decimal? HeightCm { get; set; }

    /// <summary>
    /// Device / sensor metadata.  Flexible key-value pairs:
    /// e.g. { "deviceModel": "BP-2200", "batteryLevel": "80" }
    /// </summary>
    [BsonElement("deviceMeta")]
    public Dictionary<string, string>? DeviceMeta { get; set; }

    // ── Computed for query convenience ─────────────────────────────────────────

    [BsonElement("bmi")]
    public decimal? Bmi { get; set; }

    /// <summary>Computed: SystolicBP/DiastolicBP as string "120/80".</summary>
    [BsonElement("bloodPressureText")]
    public string BloodPressureText => $"{SystolicBP}/{DiastolicBP}";
}
