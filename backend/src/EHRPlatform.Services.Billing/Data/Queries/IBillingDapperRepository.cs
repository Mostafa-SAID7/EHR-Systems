#nullable enable

namespace EHRPlatform.Services.Billing.Data.Queries;

/// <summary>
/// Dapper-backed read repository for billing reporting and financial aggregations.
///
/// Why Dapper here:
///   Invoice and payment reports require multi-table aggregation with
///   date-bucketing, running totals, and aging analysis that EF Core cannot
///   translate efficiently.  Raw parameterised SQL via Dapper gives full query
///   control while sharing the Npgsql connection owned by the EF Core BillingContext.
/// </summary>
public interface IBillingDapperRepository
{
    /// <summary>
    /// Revenue summary grouped by period (day/week/month based on granularity).
    /// Returns total billed, total collected, and outstanding balance per bucket.
    /// </summary>
    Task<IEnumerable<RevenueSummaryDto>> GetRevenueSummaryAsync(
        DateTime   from,
        DateTime   to,
        string     granularity = "month",  // day | week | month
        CancellationToken ct = default);

    /// <summary>
    /// Aging report: outstanding invoices bucketed by days since issue
    /// (0–30, 31–60, 61–90, 90+ days).
    /// </summary>
    Task<IEnumerable<AgingBucketDto>> GetInvoiceAgingAsync(
        CancellationToken ct = default);

    /// <summary>
    /// Insurance claim status summary: counts and totals by claim status.
    /// </summary>
    Task<IEnumerable<ClaimStatusSummaryDto>> GetClaimStatusSummaryAsync(
        DateTime   from,
        DateTime   to,
        CancellationToken ct = default);

    /// <summary>
    /// Top-N patients by outstanding balance.
    /// Used for collections prioritisation.
    /// </summary>
    Task<IEnumerable<PatientBalanceSummaryDto>> GetTopOutstandingBalancesAsync(
        int topN = 50,
        CancellationToken ct = default);
}

// ── DTOs ─────────────────────────────────────────────────────────────────────

public record RevenueSummaryDto(
    DateTime Period,
    decimal  TotalBilled,
    decimal  TotalCollected,
    decimal  OutstandingBalance,
    int      InvoiceCount);

public record AgingBucketDto(
    string  Bucket,        // "0-30", "31-60", "61-90", "90+"
    int     InvoiceCount,
    decimal TotalAmount,
    decimal AverageAmount);

public record ClaimStatusSummaryDto(
    string  Status,
    int     ClaimCount,
    decimal TotalAmount,
    decimal AverageAmount);

public record PatientBalanceSummaryDto(
    Guid    PatientId,
    int     OpenInvoices,
    decimal TotalOutstanding,
    DateTime OldestInvoiceDate);
