#nullable enable

using EHRPlatform.BuildingBlocks.Common.Data;
using MongoDB.Bson.Serialization.Attributes;

namespace EHRPlatform.Services.Clinical.Data.Documents;

/// <summary>
/// MongoDB document for a ClinicalNote — PRIMARY store for clinical data.
///
/// Why MongoDB:
///   A clinical encounter is a document, not a row. VitalSigns, Diagnoses,
///   and Procedures naturally embed inside the note — they are meaningless
///   without their parent note and are always queried together. Storing them
///   as separate Postgres tables forces costly JOINs on every read and adds
///   migration overhead when note schemas evolve (structured reason codes,
///   multimedia references, genomic annotations, etc.).
///
///   PatientId / ProviderId are indexed for fast per-patient timelines.
///   EntityId = ClinicalNote domain Id (Guid).
/// </summary>
public class ClinicalNoteDocument : MongoBaseDocument
{
    // ── Identity / denormalised keys ──────────────────────────────────────────
    public Guid ClinicalNoteId { get => EntityId; set => EntityId = value; }
    public Guid PatientId  { get; set; }
    public Guid ProviderId { get; set; }

    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime EncounterDate { get; set; }
    public string EncounterType { get; set; } = string.Empty;
    public string Status { get; set; } = "Draft";   // Draft | Finalized

    // ── SOAP narrative ────────────────────────────────────────────────────────
    public string Subjective  { get; set; } = string.Empty;
    public string Objective   { get; set; } = string.Empty;
    public string Assessment  { get; set; } = string.Empty;
    public string Plan        { get; set; } = string.Empty;

    // ── Embedded observations (replaces 3 separate PG tables) ─────────────────
    /// <summary>VitalSigns recorded during this encounter.</summary>
    public List<VitalSignsEmbedded> VitalSigns { get; set; } = new();

    /// <summary>ICD-10 diagnoses linked to this encounter.</summary>
    public List<DiagnosisEmbedded> Diagnoses { get; set; } = new();

    /// <summary>CPT/SNOMED procedures performed during this encounter.</summary>
    public List<ProcedureEmbedded> Procedures { get; set; } = new();

    // ── Extended content ──────────────────────────────────────────────────────
    public List<NoteAddendum>     Addenda     { get; set; } = new();
    public List<NoteAttachmentRef> Attachments { get; set; } = new();
    public Dictionary<string, string> Extensions { get; set; } = new();
}

// ── Embedded sub-documents ─────────────────────────────────────────────────────

public class VitalSignsEmbedded
{
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime RecordedAt    { get; set; } = DateTime.UtcNow;
    public decimal  Temperature   { get; set; }
    public int      SystolicBP    { get; set; }
    public int      DiastolicBP   { get; set; }
    public int      HeartRate     { get; set; }
    public int      RespiratoryRate { get; set; }
    public decimal? Weight        { get; set; }
    public int?     OxygenSaturation { get; set; }
}

public class DiagnosisEmbedded
{
    public string DiagnosisCode { get; set; } = string.Empty;
    public string DiagnosisText { get; set; } = string.Empty;
    public string DiagnosisType { get; set; } = string.Empty;  // Primary | Secondary
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime RecordedAt  { get; set; } = DateTime.UtcNow;
}

public class ProcedureEmbedded
{
    public string   ProcedureName { get; set; } = string.Empty;
    public string   ProcedureCode { get; set; } = string.Empty;
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime PerformedAt   { get; set; }
    public string   Result        { get; set; } = string.Empty;
}

public class NoteAddendum
{
    public Guid     ProviderId { get; set; }
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime AddedAt    { get; set; }
    public string   Content    { get; set; } = string.Empty;
    public string   Reason     { get; set; } = string.Empty;
}

public class NoteAttachmentRef
{
    public string   FileName    { get; set; } = string.Empty;
    public string   ContentType { get; set; } = string.Empty;
    public long     SizeBytes   { get; set; }
    public string   StorageKey  { get; set; } = string.Empty;
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime UploadedAt  { get; set; }
    public Guid     UploadedBy  { get; set; }
}

