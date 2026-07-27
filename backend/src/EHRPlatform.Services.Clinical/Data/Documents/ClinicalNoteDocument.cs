#nullable enable

using EHRPlatform.Common.Data;
using MongoDB.Bson.Serialization.Attributes;

namespace EHRPlatform.Services.Clinical.Data.Documents;

/// <summary>
/// MongoDB document that stores the full SOAP content of a clinical note.
///
/// Design rationale — dual-store pattern:
///   PostgreSQL  → structured metadata (NoteId, PatientId, Status, EncounterDate,
///                 ICD-10 codes, CPT procedures). Relational integrity, FK support.
///   MongoDB     → unstructured SOAP narrative (Subjective, Objective, Assessment,
///                 Plan). Unbounded text; schema-flexible for future HL7 extensions.
///
/// The two stores are linked by <see cref="NoteId"/> (the PostgreSQL PK).
/// On write: save the EF entity first, then upsert this document using NoteId.
/// On read: fetch the EF entity for metadata; join this document for full content.
/// </summary>
public class ClinicalNoteDocument : MongoBaseDocument
{
    /// <summary>
    /// The PostgreSQL ClinicalNote.Id — join key between stores.
    /// Stored as a separate field (not just EntityId) to be explicit.
    /// </summary>
    [BsonElement("noteId")]
    public Guid NoteId { get; set; }

    [BsonElement("patientId")]
    public Guid PatientId { get; set; }

    [BsonElement("providerId")]
    public Guid ProviderId { get; set; }

    /// <summary>
    /// Encounter date — duplicated here to support time-range filtering without
    /// round-tripping to PostgreSQL.
    /// </summary>
    [BsonElement("encounterDate")]
    public DateTime EncounterDate { get; set; }

    /// <summary>
    /// Note status: Draft | Finalized | Locked.
    /// Mirrors the PostgreSQL field for query convenience.
    /// </summary>
    [BsonElement("status")]
    public string Status { get; set; } = "Draft";

    // ── SOAP components (unbounded narrative text) ────────────────────────────

    /// <summary>
    /// Subjective — patient's reported symptoms, complaints, and history.
    /// No length limit; free-form clinical narrative.
    /// </summary>
    [BsonElement("subjective")]
    public string Subjective { get; set; } = string.Empty;

    /// <summary>
    /// Objective — clinician observations, physical exam findings, lab results.
    /// </summary>
    [BsonElement("objective")]
    public string Objective { get; set; } = string.Empty;

    /// <summary>
    /// Assessment — diagnosis and clinical impression.
    /// </summary>
    [BsonElement("assessment")]
    public string Assessment { get; set; } = string.Empty;

    /// <summary>
    /// Plan — treatment decisions, medications ordered, follow-up instructions.
    /// </summary>
    [BsonElement("plan")]
    public string Plan { get; set; } = string.Empty;

    // ── Schema versioning ─────────────────────────────────────────────────────
    // Inherited SchemaVersion from MongoBaseDocument (starts at 1).
    // Increment and migrate via MongoMigrationExecutor when SOAP shape changes.
}
