#nullable enable

using EHRPlatform.BuildingBlocks.Common.Data;

namespace EHRPlatform.Services.Audit.Data.Queries;

/// <summary>
/// Dapper-backed implementation of <see cref="IAuditDapperRepository"/>.
/// All queries are read-only (SELECT only).  Uses parameterised SQL exclusively.
/// Connection is owned by the EF Core AuditContext and participates in its
/// transaction when one is open.
/// </summary>
public sealed class AuditDapperRepository : IAuditDapperRepository
{
    private readonly IDapperContext _dapper;

    public AuditDapperRepository(IDapperContext dapper)
    {
        _dapper = dapper ?? throw new ArgumentNullException(nameof(dapper));
    }

    /// <inheritdoc />
    public async Task<IEnumerable<PatientAccessReportDto>> GetPatientAccessReportAsync(
        Guid      patientId,
        DateTime  from,
        DateTime  to,
        CancellationToken ct = default)
    {
        const string sql = """
            SELECT
                al.id              AS "AccessLogId",
                al.patient_id      AS "PatientId",
                al.user_id         AS "UserId",
                al.user_name       AS "UserName",
                al.action          AS "Action",
                al.resource_type   AS "ResourceType",
                al.accessed_at     AS "AccessedAt",
                al.ip_address      AS "IpAddress",
                al.was_successful  AS "WasSuccessful"
            FROM access_logs al
            WHERE al.patient_id = @PatientId
              AND al.accessed_at >= @From
              AND al.accessed_at <  @To
              AND al.deleted_at IS NULL
            ORDER BY al.accessed_at DESC;
            """;

        return await _dapper.QueryAsync<PatientAccessReportDto>(
            sql,
            new { PatientId = patientId, From = from, To = to },
            ct);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<DailyAuditSummaryDto>> GetDailyAuditSummaryAsync(
        DateTime  from,
        DateTime  to,
        CancellationToken ct = default)
    {
        const string sql = """
            SELECT
                date_trunc('day', ae.created_at)                           AS "Day",
                COUNT(DISTINCT ae.id)                                      AS "TotalAccesses",
                COUNT(DISTINCT dc.id)                                      AS "TotalChanges",
                COUNT(DISTINCT al.id) FILTER (WHERE NOT al.was_successful) AS "FailedAttempts",
                COUNT(DISTINCT ae.performed_by)                            AS "UniqueUsers"
            FROM audit_entries ae
            LEFT JOIN data_change_audits dc
                ON dc.audit_entry_id = ae.id
            LEFT JOIN access_logs al
                ON date_trunc('day', al.accessed_at) = date_trunc('day', ae.created_at)
            WHERE ae.created_at >= @From
              AND ae.created_at <  @To
              AND ae.deleted_at IS NULL
            GROUP BY date_trunc('day', ae.created_at)
            ORDER BY "Day" DESC;
            """;

        return await _dapper.QueryAsync<DailyAuditSummaryDto>(
            sql,
            new { From = from, To = to },
            ct);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<ProviderAccessSummaryDto>> GetTopProviderAccessAsync(
        DateTime  from,
        DateTime  to,
        int       topN = 20,
        CancellationToken ct = default)
    {
        const string sql = """
            SELECT
                al.user_id                      AS "UserId",
                al.user_name                    AS "UserName",
                COUNT(*)::bigint                AS "TotalAccesses",
                COUNT(DISTINCT al.patient_id)   AS "UniquePatients",
                MIN(al.accessed_at)             AS "FirstAccess",
                MAX(al.accessed_at)             AS "LastAccess"
            FROM access_logs al
            WHERE al.accessed_at >= @From
              AND al.accessed_at <  @To
              AND al.deleted_at IS NULL
            GROUP BY al.user_id, al.user_name
            ORDER BY "TotalAccesses" DESC
            LIMIT @TopN;
            """;

        return await _dapper.QueryAsync<ProviderAccessSummaryDto>(
            sql,
            new { From = from, To = to, TopN = topN },
            ct);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<EntityAuditTrailDto>> GetEntityAuditTrailAsync(
        Guid      entityId,
        string    entityType,
        CancellationToken ct = default)
    {
        const string sql = """
            SELECT
                ae.id             AS "AuditEntryId",
                ae.entity_id      AS "EntityId",
                ae.entity_type    AS "EntityType",
                ae.action         AS "Action",
                dc.change_details AS "ChangeDetails",
                ae.performed_by   AS "PerformedBy",
                ae.created_at     AS "PerformedAt",
                ae.ip_address     AS "IpAddress"
            FROM audit_entries ae
            LEFT JOIN data_change_audits dc ON dc.audit_entry_id = ae.id
            WHERE ae.entity_id   = @EntityId
              AND ae.entity_type = @EntityType
              AND ae.deleted_at IS NULL
            ORDER BY ae.created_at DESC;
            """;

        return await _dapper.QueryAsync<EntityAuditTrailDto>(
            sql,
            new { EntityId = entityId, EntityType = entityType },
            ct);
    }
}

