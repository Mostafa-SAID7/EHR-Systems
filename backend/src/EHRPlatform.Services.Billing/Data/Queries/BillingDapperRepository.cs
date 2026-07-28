#nullable enable

using EHRPlatform.Common.Data;

namespace EHRPlatform.Services.Billing.Data.Queries;

/// <summary>
/// Dapper-backed implementation of <see cref="IBillingDapperRepository"/>.
/// All queries are read-only.  Parameterised SQL only — no string interpolation.
/// </summary>
public sealed class BillingDapperRepository : IBillingDapperRepository
{
    private readonly IDapperContext _dapper;

    public BillingDapperRepository(IDapperContext dapper)
    {
        _dapper = dapper ?? throw new ArgumentNullException(nameof(dapper));
    }

    /// <inheritdoc />
    public async Task<IEnumerable<RevenueSummaryDto>> GetRevenueSummaryAsync(
        DateTime  from,
        DateTime  to,
        string    granularity = "month",
        CancellationToken ct = default)
    {
        // date_trunc granularity is safe here because we validate the enum above.
        // We cannot parameterise the granularity string in date_trunc, so we
        // white-list it before interpolating.
        granularity = granularity.ToLowerInvariant() switch
        {
            "day"   => "day",
            "week"  => "week",
            "month" => "month",
            _       => "month"
        };

        var sql = $"""
            SELECT
                date_trunc('{granularity}', i.issued_at)  AS "Period",
                SUM(i.total_amount)::numeric               AS "TotalBilled",
                SUM(COALESCE(p.total_paid, 0))::numeric    AS "TotalCollected",
                SUM(i.total_amount - COALESCE(p.total_paid, 0))::numeric AS "OutstandingBalance",
                COUNT(i.id)::int                           AS "InvoiceCount"
            FROM invoices i
            LEFT JOIN (
                SELECT invoice_id, SUM(amount) AS total_paid
                FROM payments
                WHERE status = 'Completed'
                  AND deleted_at IS NULL
                GROUP BY invoice_id
            ) p ON p.invoice_id = i.id
            WHERE i.issued_at >= @From
              AND i.issued_at <  @To
              AND i.deleted_at IS NULL
            GROUP BY date_trunc('{granularity}', i.issued_at)
            ORDER BY "Period" DESC;
            """;

        return await _dapper.QueryAsync<RevenueSummaryDto>(
            sql,
            new { From = from, To = to },
            ct);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<AgingBucketDto>> GetInvoiceAgingAsync(
        CancellationToken ct = default)
    {
        const string sql = """
            SELECT
                CASE
                    WHEN age_days <=  30 THEN '0-30'
                    WHEN age_days <=  60 THEN '31-60'
                    WHEN age_days <=  90 THEN '61-90'
                    ELSE '90+'
                END                    AS "Bucket",
                COUNT(*)::int          AS "InvoiceCount",
                SUM(outstanding)       AS "TotalAmount",
                AVG(outstanding)       AS "AverageAmount"
            FROM (
                SELECT
                    i.id,
                    EXTRACT(DAY FROM now() - i.issued_at)::int AS age_days,
                    i.total_amount - COALESCE(p.total_paid, 0) AS outstanding
                FROM invoices i
                LEFT JOIN (
                    SELECT invoice_id, SUM(amount) AS total_paid
                    FROM payments
                    WHERE status = 'Completed' AND deleted_at IS NULL
                    GROUP BY invoice_id
                ) p ON p.invoice_id = i.id
                WHERE i.status NOT IN ('Paid', 'Cancelled', 'Voided')
                  AND i.deleted_at IS NULL
            ) aged
            WHERE outstanding > 0
            GROUP BY "Bucket"
            ORDER BY MIN(age_days);
            """;

        return await _dapper.QueryAsync<AgingBucketDto>(sql, null, ct);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<ClaimStatusSummaryDto>> GetClaimStatusSummaryAsync(
        DateTime  from,
        DateTime  to,
        CancellationToken ct = default)
    {
        const string sql = """
            SELECT
                ic.status                    AS "Status",
                COUNT(*)::int                AS "ClaimCount",
                SUM(ic.claim_amount)         AS "TotalAmount",
                AVG(ic.claim_amount)         AS "AverageAmount"
            FROM insurance_claims ic
            WHERE ic.submitted_at >= @From
              AND ic.submitted_at <  @To
              AND ic.deleted_at IS NULL
            GROUP BY ic.status
            ORDER BY "ClaimCount" DESC;
            """;

        return await _dapper.QueryAsync<ClaimStatusSummaryDto>(
            sql,
            new { From = from, To = to },
            ct);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<PatientBalanceSummaryDto>> GetTopOutstandingBalancesAsync(
        int topN = 50,
        CancellationToken ct = default)
    {
        const string sql = """
            SELECT
                i.patient_id                                    AS "PatientId",
                COUNT(i.id)::int                                AS "OpenInvoices",
                SUM(i.total_amount - COALESCE(p.total_paid,0))  AS "TotalOutstanding",
                MIN(i.issued_at)                                AS "OldestInvoiceDate"
            FROM invoices i
            LEFT JOIN (
                SELECT invoice_id, SUM(amount) AS total_paid
                FROM payments
                WHERE status = 'Completed' AND deleted_at IS NULL
                GROUP BY invoice_id
            ) p ON p.invoice_id = i.id
            WHERE i.status NOT IN ('Paid', 'Cancelled', 'Voided')
              AND i.deleted_at IS NULL
            GROUP BY i.patient_id
            HAVING SUM(i.total_amount - COALESCE(p.total_paid, 0)) > 0
            ORDER BY "TotalOutstanding" DESC
            LIMIT @TopN;
            """;

        return await _dapper.QueryAsync<PatientBalanceSummaryDto>(
            sql,
            new { TopN = topN },
            ct);
    }
}
