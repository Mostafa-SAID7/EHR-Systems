#nullable enable

using EHRPlatform.Common.Data.Abstractions;
using EHRPlatform.Common.Data.Implementations;

namespace EHRPlatform.Services.Analytics.Data.Queries;

/// <summary>
/// Dapper-backed implementation of <see cref="IAnalyticsDapperRepository"/>.
///
/// All queries are read-only (no DML).  Connection is opened via IDapperContext
/// which wraps the existing Npgsql connection string.  Queries use parameterised
/// SQL — never string interpolation.
///
/// Time columns are stored as UTC in the database.  All DateTime parameters
/// passed in must also be UTC; callers are responsible for conversion.
/// </summary>
public class AnalyticsDapperRepository : IAnalyticsDapperRepository
{
    private readonly IDapperContext _dapper;

    public AnalyticsDapperRepository(IDapperContext dapper)
    {
        _dapper = dapper ?? throw new ArgumentNullException(nameof(dapper));
    }

    /// <inheritdoc />
    public async Task<IEnumerable<EventCountByDayDto>> GetEventCountsByDayAsync(
        string   eventType,
        DateTime from,
        DateTime to,
        CancellationToken ct = default)
    {
        const string sql = """
            SELECT
                event_type     AS "EventType",
                date_trunc('day', occurred_at) AS "Day",
                COUNT(*)::bigint               AS "Count"
            FROM event_metrics
            WHERE event_type = @EventType
              AND occurred_at >= @From
              AND occurred_at <  @To
            GROUP BY event_type, date_trunc('day', occurred_at)
            ORDER BY "Day" DESC;
            """;

        return await _dapper.QueryAsync<EventCountByDayDto>(
            sql,
            new { EventType = eventType, From = from, To = to },
            ct);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<MetricSummaryDto>> GetMetricSummariesAsync(
        DateTime periodStart,
        DateTime periodEnd,
        CancellationToken ct = default)
    {
        // Window function computes period-over-period change vs the previous period
        // of the same length.  Returns NULL for metrics with no prior-period data.
        const string sql = """
            WITH current_period AS (
                SELECT
                    metric_name,
                    category,
                    MAX(value)  AS latest_value,
                    MIN(value)  AS min_value,
                    MAX(value)  AS max_value,
                    AVG(value)  AS avg_value,
                    MAX(recorded_at) AS last_recorded
                FROM analytics_metrics
                WHERE recorded_at >= @PeriodStart
                  AND recorded_at <  @PeriodEnd
                GROUP BY metric_name, category
            ),
            prior_period AS (
                SELECT
                    metric_name,
                    category,
                    AVG(value) AS avg_value
                FROM analytics_metrics
                WHERE recorded_at >= (@PeriodStart - (@PeriodEnd - @PeriodStart))
                  AND recorded_at <  @PeriodStart
                GROUP BY metric_name, category
            )
            SELECT
                c.metric_name                  AS "MetricName",
                c.category                     AS "Category",
                c.latest_value                 AS "LatestValue",
                c.min_value                    AS "MinValue",
                c.max_value                    AS "MaxValue",
                c.avg_value                    AS "AvgValue",
                CASE
                    WHEN p.avg_value IS NULL OR p.avg_value = 0 THEN NULL
                    ELSE ROUND(((c.avg_value - p.avg_value) / p.avg_value * 100)::numeric, 2)
                END                            AS "PeriodChangePercent"
            FROM current_period c
            LEFT JOIN prior_period p
                ON c.metric_name = p.metric_name
               AND c.category    = p.category
            ORDER BY c.metric_name;
            """;

        return await _dapper.QueryAsync<MetricSummaryDto>(
            sql,
            new { PeriodStart = periodStart, PeriodEnd = periodEnd },
            ct);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<MetricTimeSeriesDto>> GetMetricTimeSeriesAsync(
        string   metricName,
        DateTime from,
        DateTime to,
        CancellationToken ct = default)
    {
        // Running total uses a window SUM over ordered rows.
        const string sql = """
            SELECT
                recorded_at                                              AS "RecordedAt",
                value                                                    AS "Value",
                SUM(value) OVER (ORDER BY recorded_at ROWS UNBOUNDED PRECEDING)
                                                                         AS "RunningTotal"
            FROM analytics_metrics
            WHERE metric_name = @MetricName
              AND recorded_at >= @From
              AND recorded_at <  @To
            ORDER BY recorded_at;
            """;

        return await _dapper.QueryAsync<MetricTimeSeriesDto>(
            sql,
            new { MetricName = metricName, From = from, To = to },
            ct);
    }

    /// <inheritdoc />
    public async Task<TopNSummaryDto> GetTopNSummaryAsync(
        DateTime from,
        DateTime to,
        int topN         = 10,
        CancellationToken ct = default)
    {
        // Three result sets in one round-trip using QueryMultiple.
        const string sql = """
            SELECT COUNT(*)::bigint AS "TotalCount"
            FROM event_metrics
            WHERE occurred_at >= @From AND occurred_at < @To;

            SELECT event_type AS "EventType", COUNT(*)::bigint AS "TotalCount"
            FROM event_metrics
            WHERE occurred_at >= @From AND occurred_at < @To
            GROUP BY event_type
            ORDER BY "TotalCount" DESC
            LIMIT @TopN;

            SELECT aggregate_id AS "AggregateId", event_type AS "EventType",
                   COUNT(*)::bigint AS "EventCount"
            FROM event_metrics
            WHERE occurred_at >= @From AND occurred_at < @To
            GROUP BY aggregate_id, event_type
            ORDER BY "EventCount" DESC
            LIMIT @TopN;
            """;

        return await _dapper.QueryMultipleAsync<TopNSummaryDto>(
            sql,
            async grid =>
            {
                var totalRow   = await grid.ReadFirstAsync<dynamic>();
                var topTypes   = (await grid.ReadAsync<TopNEventTypeDto>()).ToList();
                var topAggs    = (await grid.ReadAsync<TopNAggregateDto>()).ToList();
                long total     = (long)(totalRow.TotalCount ?? 0L);
                return new TopNSummaryDto(total, topTypes, topAggs);
            },
            new { From = from, To = to, TopN = topN },
            ct);
    }
}

