#nullable enable

namespace EHRPlatform.Services.Analytics.Data.Queries;

/// <summary>
/// Dapper-backed analytics query repository for complex, aggregation-heavy
/// SQL that EF Core's LINQ translator produces poorly.
///
/// All methods are read-only (SELECT only — no DML).  Use the EF Core
/// AnalyticsContext via IUnitOfWork for writes.
///
/// Why Dapper here instead of EF Core?
///   - Window functions (LAG, LEAD, ROW_NUMBER) for trend analysis.
///   - GROUP BY ROLLUP / CUBE for multi-dimensional aggregates.
///   - CTE chains for running totals and period-over-period comparisons.
///   - Direct JSONB operator access on EventMetric.Properties without
///     round-tripping through EF Core's limited JSON support.
/// </summary>
public interface IAnalyticsDapperRepository
{
    /// <summary>
    /// Count events by type within a time window, grouped by day.
    /// Returns: [ { EventType, Day, Count } ]
    /// </summary>
    Task<IEnumerable<EventCountByDayDto>> GetEventCountsByDayAsync(
        string      eventType,
        DateTime    from,
        DateTime    to,
        CancellationToken ct = default);

    /// <summary>
    /// Get a summary of all metric categories with their latest values,
    /// min/max, and period-over-period change percentage.
    /// Returns one row per (MetricName, Category) combination.
    /// </summary>
    Task<IEnumerable<MetricSummaryDto>> GetMetricSummariesAsync(
        DateTime    periodStart,
        DateTime    periodEnd,
        CancellationToken ct = default);

    /// <summary>
    /// Running total of a named metric over a date range (useful for charts).
    /// Returns: [ { RecordedAt, Value, RunningTotal } ]
    /// </summary>
    Task<IEnumerable<MetricTimeSeriesDto>> GetMetricTimeSeriesAsync(
        string      metricName,
        DateTime    from,
        DateTime    to,
        CancellationToken ct = default);

    /// <summary>
    /// Top-N aggregates in a single round-trip using Dapper GridReader.
    /// Returns: total event count + top EventTypes + top AggregateIds.
    /// </summary>
    Task<TopNSummaryDto> GetTopNSummaryAsync(
        DateTime    from,
        DateTime    to,
        int         topN     = 10,
        CancellationToken ct = default);
}

// ── DTOs (query result shapes) ─────────────────────────────────────────────────

public record EventCountByDayDto(
    string   EventType,
    DateTime Day,
    long     Count);

public record MetricSummaryDto(
    string   MetricName,
    string?  Category,
    decimal  LatestValue,
    decimal  MinValue,
    decimal  MaxValue,
    decimal  AvgValue,
    decimal? PeriodChangePercent);

public record MetricTimeSeriesDto(
    DateTime RecordedAt,
    decimal  Value,
    decimal  RunningTotal);

public record TopNEventTypeDto(
    string EventType,
    long   TotalCount);

public record TopNAggregateDto(
    Guid   AggregateId,
    string EventType,
    long   EventCount);

public record TopNSummaryDto(
    long                        TotalEventCount,
    IReadOnlyList<TopNEventTypeDto>  TopEventTypes,
    IReadOnlyList<TopNAggregateDto>  TopAggregates);
