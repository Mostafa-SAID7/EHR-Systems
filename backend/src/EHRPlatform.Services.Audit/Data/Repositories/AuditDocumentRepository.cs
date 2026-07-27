#nullable enable

using EHRPlatform.Common.Data;
using EHRPlatform.Services.Audit.Data.Documents;
using MongoDB.Driver;

namespace EHRPlatform.Services.Audit.Data.Repositories;

/// <summary>
/// MongoDB implementation of <see cref="IAuditDocumentRepository"/>.
///
/// Collections:
///   "audit-entry-documents"  — HIPAA audit trail entries (immutable).
///   "access-log-documents"   — resource access log entries (immutable).
///
/// All inserts are append-only.  There are intentionally no Update or Delete
/// methods — audit logs are legally immutable for 7 years (HIPAA §164.530).
/// </summary>
public class AuditDocumentRepository : IAuditDocumentRepository
{
    private readonly IMongoCollection<AuditEntryDocument> _auditEntries;
    private readonly IMongoCollection<AccessLogDocument>  _accessLogs;

    public AuditDocumentRepository(IMongoDatabase database)
    {
        ArgumentNullException.ThrowIfNull(database);

        _auditEntries = database.GetCollection<AuditEntryDocument>("audit-entry-documents");
        _accessLogs   = database.GetCollection<AccessLogDocument>("access-log-documents");

        // Ensure indexes exist (idempotent — MongoDB ignores duplicates)
        EnsureIndexesAsync().GetAwaiter().GetResult();
    }

    // ── Index setup ────────────────────────────────────────────────────────────

    private async Task EnsureIndexesAsync()
    {
        // AuditEntry indexes
        var auditIndexes = new[]
        {
            new CreateIndexModel<AuditEntryDocument>(
                Builders<AuditEntryDocument>.IndexKeys
                    .Ascending(d => d.UserId)
                    .Descending(d => d.Timestamp),
                new CreateIndexOptions { Name = "IX_AuditEntry_UserId_Timestamp" }),

            new CreateIndexModel<AuditEntryDocument>(
                Builders<AuditEntryDocument>.IndexKeys
                    .Ascending(d => d.ResourceType)
                    .Ascending(d => d.ResourceId)
                    .Descending(d => d.Timestamp),
                new CreateIndexOptions { Name = "IX_AuditEntry_Resource_Timestamp" }),

            new CreateIndexModel<AuditEntryDocument>(
                Builders<AuditEntryDocument>.IndexKeys.Ascending(d => d.Action),
                new CreateIndexOptions { Name = "IX_AuditEntry_Action" }),

            new CreateIndexModel<AuditEntryDocument>(
                Builders<AuditEntryDocument>.IndexKeys.Descending(d => d.Timestamp),
                new CreateIndexOptions { Name = "IX_AuditEntry_Timestamp" }),
        };

        await _auditEntries.Indexes.CreateManyAsync(auditIndexes);

        // AccessLog indexes
        var accessIndexes = new[]
        {
            new CreateIndexModel<AccessLogDocument>(
                Builders<AccessLogDocument>.IndexKeys
                    .Ascending(d => d.ResourceType)
                    .Ascending(d => d.ResourceId)
                    .Descending(d => d.AccessedAt),
                new CreateIndexOptions { Name = "IX_AccessLog_Resource_AccessedAt" }),

            new CreateIndexModel<AccessLogDocument>(
                Builders<AccessLogDocument>.IndexKeys
                    .Ascending(d => d.UserId)
                    .Descending(d => d.AccessedAt),
                new CreateIndexOptions { Name = "IX_AccessLog_UserId_AccessedAt" }),
        };

        await _accessLogs.Indexes.CreateManyAsync(accessIndexes);
    }

    // ── AuditEntry ─────────────────────────────────────────────────────────────

    public async Task AppendAuditEntryAsync(
        AuditEntryDocument entry,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entry);
        entry.CreatedAt = DateTime.UtcNow;
        entry.UpdatedAt = DateTime.UtcNow;
        await _auditEntries.InsertOneAsync(entry, null, cancellationToken);
    }

    public async Task AppendAuditEntriesAsync(
        IEnumerable<AuditEntryDocument> entries,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entries);
        var list = entries.ToList();
        if (list.Count == 0) return;

        var now = DateTime.UtcNow;
        foreach (var e in list) { e.CreatedAt = now; e.UpdatedAt = now; }
        await _auditEntries.InsertManyAsync(list, null, cancellationToken);
    }

    public async Task<(IEnumerable<AuditEntryDocument> entries, long totalCount)>
        GetAuditEntriesForUserAsync(
            Guid userId,
            int pageNumber,
            int pageSize,
            DateTime? from          = null,
            DateTime? to            = null,
            CancellationToken ct    = default)
    {
        var filter = Builders<AuditEntryDocument>.Filter.Eq(d => d.UserId, userId);

        if (from.HasValue)
            filter &= Builders<AuditEntryDocument>.Filter.Gte(d => d.Timestamp, from.Value);
        if (to.HasValue)
            filter &= Builders<AuditEntryDocument>.Filter.Lte(d => d.Timestamp, to.Value);

        var total = await _auditEntries.CountDocumentsAsync(filter, null, ct);
        var items = await _auditEntries.Find(filter)
            .SortByDescending(d => d.Timestamp)
            .Skip((pageNumber - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync(ct);

        return (items, total);
    }

    public async Task<(IEnumerable<AuditEntryDocument> entries, long totalCount)>
        GetAuditEntriesForResourceAsync(
            string resourceType,
            Guid resourceId,
            int pageNumber,
            int pageSize,
            CancellationToken ct    = default)
    {
        var filter = Builders<AuditEntryDocument>.Filter.And(
            Builders<AuditEntryDocument>.Filter.Eq(d => d.ResourceType, resourceType),
            Builders<AuditEntryDocument>.Filter.Eq(d => d.ResourceId, resourceId));

        var total = await _auditEntries.CountDocumentsAsync(filter, null, ct);
        var items = await _auditEntries.Find(filter)
            .SortByDescending(d => d.Timestamp)
            .Skip((pageNumber - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync(ct);

        return (items, total);
    }

    public async Task<bool> VerifyIntegrityAsync(
        string documentId,
        string computedHash,
        CancellationToken ct = default)
    {
        var filter = Builders<AuditEntryDocument>.Filter.Eq(d => d.Id, documentId);
        var doc    = await _auditEntries.Find(filter).FirstOrDefaultAsync(ct);
        return doc != null && doc.IntegrityHash == computedHash;
    }

    // ── AccessLog ──────────────────────────────────────────────────────────────

    public async Task AppendAccessLogAsync(
        AccessLogDocument log,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(log);
        log.CreatedAt = DateTime.UtcNow;
        log.UpdatedAt = DateTime.UtcNow;
        await _accessLogs.InsertOneAsync(log, null, cancellationToken);
    }

    public async Task AppendAccessLogsAsync(
        IEnumerable<AccessLogDocument> logs,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(logs);
        var list = logs.ToList();
        if (list.Count == 0) return;

        var now = DateTime.UtcNow;
        foreach (var l in list) { l.CreatedAt = now; l.UpdatedAt = now; }
        await _accessLogs.InsertManyAsync(list, null, cancellationToken);
    }

    public async Task<(IEnumerable<AccessLogDocument> logs, long totalCount)>
        GetAccessLogsForResourceAsync(
            string resourceType,
            Guid resourceId,
            int pageNumber,
            int pageSize,
            CancellationToken ct = default)
    {
        var filter = Builders<AccessLogDocument>.Filter.And(
            Builders<AccessLogDocument>.Filter.Eq(d => d.ResourceType, resourceType),
            Builders<AccessLogDocument>.Filter.Eq(d => d.ResourceId,   resourceId));

        var total = await _accessLogs.CountDocumentsAsync(filter, null, ct);
        var items = await _accessLogs.Find(filter)
            .SortByDescending(d => d.AccessedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync(ct);

        return (items, total);
    }
}
