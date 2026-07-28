#nullable enable

using EHRPlatform.Common.Data;

namespace EHRPlatform.Services.Clinical.Data.Documents;

/// <summary>
/// MongoDB document representation of a clinical note.
///
/// Why MongoDB for this entity:
///   SOAP notes (Subjective / Objective / Assessment / Plan) are free-form
///   rich text with no fixed column width, highly variable length, and
///   provider-specific schemas.  Storing them as TEXT in PostgreSQL works
///   but loses the ability to:
///     - Embed attachments / addenda as sub-documents
///     - Query across flexible section structures without schema migrations
///     - Store future discrete data (structured reason-codes, multimedia refs)
///       alongside the free text without table alterations
///
///   The EntityId links back to the canonical ClinicalNote row in PostgreSQL
///   (PatientId, ProviderId, EncounterDate, Status) so joins are possible.
///   Structured, indexed data stays relational; rich text content lives here.
/// </summary>
public class ClinicalNoteDocument : MongoBaseDocument
{
    // ── Link to PostgreSQL canonical record ───────────────────────────────────

    /// <summary>The ClinicalNote.Id from the PostgreSQL ClinicalContext.</summary>
    public Guid ClinicalNoteId
    {
        get => EntityId;
        set => EntityId = value;
    }

    /// <summary>Denormalized for fast document-only queries.</summary>
    public Guid PatientId { get; set; }

    /// <summary>Denormalized for fast document-only queries.</summary>
    public Guid ProviderId { get; set; }

    public DateTime EncounterDate { get; set; }

    // ── SOAP free-text content ────────────────────────────────────────────────

    /// <summary>Subjective: patient complaint and symptom history in provider's own words.</summary>
    public string Subjective { get; set; } = string.Empty;

    /// <summary>Objective: physical exam findings, vital-sign narratives, lab interpretations.</summary>
    public string Objective { get; set; } = string.Empty;

    /// <summary>Assessment: differential diagnosis and clinical impression.</summary>
    public string Assessment { get; set; } = string.Empty;

    /// <summary>Plan: treatment decisions, prescriptions, referrals, follow-up instructions.</summary>
    public string Plan { get; set; } = string.Empty;

    // ── Extended content (document-model advantages) ──────────────────────────

    /// <summary>
    /// Addenda appended after the note was finalized (common in signed notes).
    /// Each addendum is timestamped and attributed to a provider.
    /// </summary>
    public List<NoteAddendum> Addenda { get; set; } = new();

    /// <summary>
    /// References to attached files (scanned consents, wound photos, ECG strips).
    /// Stored as metadata only — binary content lives in object storage.
    /// </summary>
    public List<NoteAttachmentRef> Attachments { get; set; } = new();

    /// <summary>
    /// Arbitrary provider-defined key/value extensions.
    /// Avoids schema migrations for clinic-specific discrete fields.
    /// Examples: "telehealthPlatform" → "Zoom", "interpretingLanguage" → "Arabic"
    /// </summary>
    public Dictionary<string, string> Extensions { get; set; } = new();
}

/// <summary>An addendum appended to a finalized clinical note.</summary>
public class NoteAddendum
{
    public Guid ProviderId { get; set; }
    public DateTime AddedAt { get; set; }
    public string Content { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty; // Correction, Clarification, LateLab
}

/// <summary>Metadata reference to a file attached to a clinical note.</summary>
public class NoteAttachmentRef
{
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty; // image/jpeg, application/pdf …
    public long SizeBytes { get; set; }
    public string StorageKey { get; set; } = string.Empty; // S3 / object-storage key
    public DateTime UploadedAt { get; set; }
    public Guid UploadedBy { get; set; }
}
