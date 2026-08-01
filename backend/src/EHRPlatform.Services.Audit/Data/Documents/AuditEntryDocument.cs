#nullable enable

using EHRPlatform.BuildingBlocks.Common.Data;
using MongoDB.Bson.Serialization.Attributes;

namespace EHRPlatform.Services.Audit.Data.Documents;

/// <summary>
/// MongoDB document for a HIPAA audit entry.
///
/// Design rationale — MongoDB as primary store for audit logs:
///   - Audit entries are append-only (immutable by HIPAA regulation).
///   - Volume is high: every API call that touches PHI produces at least one entry.
///   - Schema is semi-structured: ChangeDetails is a JSON diff whose shape depends
///     on the resource type (Patient vs Invoice vs ClinicalNote).
///   - MongoDB shards naturally on UserId + Timestamp for compliance queries.
///   - The PostgreSQL AuditEntry table is kept as a secondary "compliance summary"
///     store for reporting workflows that need JOINs (e.g. ComplianceReport).
///
/// Integrity hash (SHA-256) is stored alongside the document payload so that
/// tamper detection works even if the document is read back without decryption.
/// </summary>
public class AuditEntryDocument : MongoBaseDocument
{
    /// <summary>
    /// Links to the PostgreSQL AuditEntry.Id for cross-store joins.
    /// Null when the entry was written directly to MongoDB without a PG row.
    /// </summary>
    [BsonElement("pgEntryId")]
    public Guid? PgEntryId { get; set; }

    [BsonElement("userId")]
    public Guid UserId { get; set; }

    [BsonElement("userEmail")]
    public string UserEmail { get; set; } = string.Empty;

    /// <summary>Create | Read | Update | Delete | Export | Print | Login | Logout</summary>
    [BsonElement("action")]
    public string Action { get; set; } = string.Empty;

    /// <summary>Patient | ClinicalNote | Appointment | Prescription | Invoice | …</summary>
    [BsonElement("resourceType")]
    public string ResourceType { get; set; } = string.Empty;

    [BsonElement("resourceId")]
    public Guid ResourceId { get; set; }

    /// <summary>Success | Failure | PartialSuccess</summary>
    [BsonElement("status")]
    public string Status { get; set; } = string.Empty;

    [BsonElement("timestamp")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime Timestamp { get; set; }

    [BsonElement("ipAddress")]
    public string IpAddress { get; set; } = string.Empty;

    [BsonElement("userAgent")]
    public string UserAgent { get; set; } = string.Empty;

    /// <summary>
    /// Comma-separated PHI field indicators: SSN | DOB | MRN | Address | …
    /// Stored as text here; use <see cref="PiiIndicatorList"/> for a typed list.
    /// </summary>
    [BsonElement("piiIndicators")]
    public string? PiiIndicators { get; set; }

    /// <summary>
    /// Parsed PII indicator list — convenience property, not persisted separately.
    /// </summary>
    [BsonIgnore]
    public IReadOnlyList<string> PiiIndicatorList =>
        string.IsNullOrWhiteSpace(PiiIndicators)
            ? Array.Empty<string>()
            : PiiIndicators.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

    /// <summary>0 = Public | 1 = Audit | 2 = Clinical | 3 = Admin | 4 = Full</summary>
    [BsonElement("accessLevel")]
    public int AccessLevel { get; set; }

    /// <summary>
    /// JSON diff of field-level changes: { "fieldName": { "old": "x", "new": "y" } }
    /// Stored as a raw string to preserve exact JSON without re-serialisation loss.
    /// No length limit — audit diffs for complex resources can be large.
    /// </summary>
    [BsonElement("changeDetails")]
    public string? ChangeDetails { get; set; }

    [BsonElement("failureReason")]
    public string? FailureReason { get; set; }

    /// <summary>
    /// SHA-256 of (UserId + Action + ResourceType + ResourceId + Timestamp + ChangeDetails).
    /// Allows offline tamper detection without decrypting the full document.
    /// </summary>
    [BsonElement("integrityHash")]
    public string IntegrityHash { get; set; } = string.Empty;

    [BsonElement("sessionDurationSeconds")]
    public int? SessionDurationSeconds { get; set; }

    [BsonElement("isEncrypted")]
    public bool IsEncrypted { get; set; }

    // ── HIPAA override: audit logs must never be soft-deleted ─────────────────
    // DeletedAt from MongoBaseDocument is intentionally ignored for this type.
    // Entries are immutable from the moment they are written.
}

