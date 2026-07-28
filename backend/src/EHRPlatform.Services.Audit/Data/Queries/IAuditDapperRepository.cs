#nullable enable

namespace EHRPlatform.Services.Audit.Data.Queries;

/// <summary>
/// Dapper-backed read repository for HIPAA compliance reporting and bulk audit queries.
///
/// Why Dapper here:
///   HIPAA compliance reports require cross-entity aggregation (AuditEntry +
///   AccessLog + DataChangeAudit) with window functions and GROUP BY that EF Core
///   translates inefficiently. Raw SQL via Dapper gives full control over the
///   query plan while reusing the same Npgsql connection as the EF Core context.
/// </summary>
public interface IAuditDapperRepository
{
    /// <summary>
    /// HIPAA Access Report: who accessed what patient records in a date range.
    /// Used for breach-notification investigations and periodic access reviews.
    /// </summary>
    Task<IEnumerable<PatientAccessReportDto>> GetPatientAccessReportAsync(
        Guid       patientId,
        DateTime   from,
        DateTime   to,
        CancellationToken ct = default);

    /// <summary>
    /// Compliance summary: total accesses, change events, and failed access attempts
    /// grouped by day within the given period.
    /// </summary>
    Task<IEnumerable<DailyAuditSummaryDto>> GetDailyAuditSummaryAsync(
        DateTime   from,
        DateTime   to,
        CancellationToken ct = default);

    /// <summary>
    /// Top-N providers by number of patient record accesses within the period.
    /// Used to detect anomalous access patterns.
    /// </summary>
    Task<IEnumerable<ProviderAccessSummaryDto>> GetTopProviderAccessAsync(
        DateTime   from,
        DateTime   to,
        int        topN = 20,
        CancellationToken ct = default);

    /// <summary>
    /// Full audit trail for a single entity (any aggregate type) in reverse-chronological order.
    /// </summary>
    Task<IEnumerable<EntityAuditTrailDto>> GetEntityAuditTrailAsync(
        Guid       entityId,
        string     entityType,
        CancellationToken ct = default);
}

// ── DTOs ─────────────────────────────────────────────────────────────────────

public record PatientAccessReportDto(
    Guid     AccessLogId,
    Guid     PatientId,
    Guid     UserId,
    string   UserName,
    string   Action,
    string   ResourceType,
    DateTime AccessedAt,
    string   IpAddress,
    bool     WasSuccessful);

public record DailyAuditSummaryDto(
    DateTime Day,
    long     TotalAccesses,
    long     TotalChanges,
    long     FailedAttempts,
    long     UniqueUsers);

public record ProviderAccessSummaryDto(
    Guid     UserId,
    string   UserName,
    long     TotalAccesses,
    long     UniquePatients,
    DateTime FirstAccess,
    DateTime LastAccess);

public record EntityAuditTrailDto(
    Guid     AuditEntryId,
    Guid     EntityId,
    string   EntityType,
    string   Action,
    string?  ChangeDetails,
    Guid     PerformedBy,
    DateTime PerformedAt,
    string?  IpAddress);
