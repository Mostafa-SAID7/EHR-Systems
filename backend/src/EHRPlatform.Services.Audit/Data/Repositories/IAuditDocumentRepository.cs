#nullable enable

using EHRPlatform.Services.Audit.Data.Documents;

namespace EHRPlatform.Services.Audit.Data.Repositories;

/// <summary>
/// MongoDB-backed repository for high-volume, append-only audit records.
///
/// Audit entries and access logs are written here in addition to (or instead of)
/// the PostgreSQL AuditContext for:
///   - High write throughput without relational overhead.
///   - Flexible JSON structure for ChangeDetails diffs.
///   - Long-retention (7+ years) at lower storage cost.
///   - HIPAA tamper detection via integrity hashes.
///
/// All write operations are fire-and-forget from the service perspective —
/// failure to write to MongoDB must NOT roll back the main operation.
/// </summary>
public interface IAuditDocumentRepository
{
    // ── AuditEntry operations ──────────────────────────────────────────────────

    /// <summary>Append a new audit entry (immutable — no update/delete).</summary>
    Task AppendAuditEntryAsync(
        AuditEntryDocument entry,
        CancellationToken cancellationToken = default);

    /// <summary>Append multiple audit entries in one batch insert.</summary>
    Task AppendAuditEntriesAsync(
        IEnumerable<AuditEntryDocument> entries,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get paginated audit entries for a user, ordered newest-first.
    /// Used for compliance queries: "show all actions by user X".
    /// </summary>
    Task<(IEnumerable<AuditEntryDocument> entries, long totalCount)> GetAuditEntriesForUserAsync(
        Guid userId,
        int pageNumber,
        int pageSize,
        DateTime? from              = null,
        DateTime? to                = null,
        CancellationToken ct        = default);

    /// <summary>
    /// Get paginated audit entries for a resource (e.g. all actions on Patient P).
    /// Used for "who accessed this record?" HIPAA queries.
    /// </summary>
    Task<(IEnumerable<AuditEntryDocument> entries, long totalCount)> GetAuditEntriesForResourceAsync(
        string resourceType,
        Guid resourceId,
        int pageNumber,
        int pageSize,
        CancellationToken ct        = default);

    /// <summary>
    /// Verify the integrity hash of a specific audit entry.
    /// Returns true if the stored hash matches the computed hash (no tampering).
    /// </summary>
    Task<bool> VerifyIntegrityAsync(
        string documentId,
        string computedHash,
        CancellationToken ct        = default);

    // ── AccessLog operations ───────────────────────────────────────────────────

    /// <summary>Append a new access log entry.</summary>
    Task AppendAccessLogAsync(
        AccessLogDocument log,
        CancellationToken cancellationToken = default);

    /// <summary>Append multiple access log entries in one batch.</summary>
    Task AppendAccessLogsAsync(
        IEnumerable<AccessLogDocument> logs,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get paginated access logs for a resource, ordered newest-first.
    /// </summary>
    Task<(IEnumerable<AccessLogDocument> logs, long totalCount)> GetAccessLogsForResourceAsync(
        string resourceType,
        Guid resourceId,
        int pageNumber,
        int pageSize,
        CancellationToken ct        = default);
}
