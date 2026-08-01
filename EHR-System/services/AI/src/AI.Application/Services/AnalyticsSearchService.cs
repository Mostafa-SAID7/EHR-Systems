using Elastic.Clients.Elasticsearch;
using EHRPlatform.Services.Analytics.Domain.Entities;

namespace EHRPlatform.Services.Analytics.Application.Services;

/// <summary>
/// Analytics search service using Elasticsearch.
/// Provides full-text search on reports, metrics, dashboards.
/// Gracefully degrades if Elasticsearch unavailable.
/// </summary>
public interface IAnalyticsSearchService
{
    Task<IEnumerable<Report>> SearchReportsAsync(string query, int limit = 20, CancellationToken ct = default);
    Task<IEnumerable<AnalyticsMetric>> SearchMetricsAsync(string query, int limit = 20, CancellationToken ct = default);
    Task IndexReportAsync(Report report, CancellationToken ct = default);
    Task IndexMetricAsync(AnalyticsMetric metric, CancellationToken ct = default);
    Task<bool> IsAvailableAsync(CancellationToken ct = default);
}

public class AnalyticsSearchService : IAnalyticsSearchService
{
    private readonly ElasticsearchClient? _client;
    private readonly ILogger<AnalyticsSearchService> _logger;
    private const string ReportsIndex = "analytics-reports";
    private const string MetricsIndex = "analytics-metrics";

    public AnalyticsSearchService(ElasticsearchClient? client, ILogger<AnalyticsSearchService> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<IEnumerable<Report>> SearchReportsAsync(string query, int limit = 20, CancellationToken ct = default)
    {
        if (_client == null || string.IsNullOrWhiteSpace(query))
            return Enumerable.Empty<Report>();

        try
        {
            _logger.LogInformation("Searching reports: {Query}", query);

            var response = await _client.SearchAsync<Report>(s => s
                .Index(ReportsIndex)
                .Query(q => q
                    .MultiMatch(mm => mm
                        .Query(query)
                        .Fields(new Elastic.Clients.Elasticsearch.Field[] { "name^2", "description", "content" })))
                .Size(limit),
                ct);

            if (!response.IsValidResponse)
            {
                _logger.LogWarning("Elasticsearch search failed: {Error}", response.DebugInformation);
                return Enumerable.Empty<Report>();
            }

            return response.Documents;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error searching reports");
            return Enumerable.Empty<Report>(); // Graceful degradation
        }
    }

    public async Task<IEnumerable<AnalyticsMetric>> SearchMetricsAsync(string query, int limit = 20, CancellationToken ct = default)
    {
        if (_client == null || string.IsNullOrWhiteSpace(query))
            return Enumerable.Empty<AnalyticsMetric>();

        try
        {
            _logger.LogInformation("Searching metrics: {Query}", query);

            var response = await _client.SearchAsync<AnalyticsMetric>(s => s
                .Index(MetricsIndex)
                .Query(q => q
                    .MultiMatch(mm => mm
                        .Query(query)
                        .Fields(new Elastic.Clients.Elasticsearch.Field[] { "name^2", "description", "category" })))
                .Size(limit),
                ct);

            if (!response.IsValidResponse)
            {
                _logger.LogWarning("Elasticsearch search failed: {Error}", response.DebugInformation);
                return Enumerable.Empty<AnalyticsMetric>();
            }

            return response.Documents;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error searching metrics");
            return Enumerable.Empty<AnalyticsMetric>();
        }
    }

    public async Task IndexReportAsync(Report report, CancellationToken ct = default)
    {
        if (_client == null || report == null)
            return;

        try
        {
            await _client.IndexAsync(report, i => i
                .Index(ReportsIndex)
                .Id(report.Id.ToString()),
                ct);

            _logger.LogDebug("Indexed report: {ReportId}", report.Id);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to index report: {ReportId}", report.Id);
            // Don't fail - search is optional
        }
    }

    public async Task IndexMetricAsync(AnalyticsMetric metric, CancellationToken ct = default)
    {
        if (_client == null || metric == null)
            return;

        try
        {
            await _client.IndexAsync(metric, i => i
                .Index(MetricsIndex)
                .Id(metric.Id.ToString()),
                ct);

            _logger.LogDebug("Indexed metric: {MetricId}", metric.Id);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to index metric: {MetricId}", metric.Id);
        }
    }

    public async Task<bool> IsAvailableAsync(CancellationToken ct = default)
    {
        if (_client == null)
            return false;

        try
        {
            var response = await _client.PingAsync(ct);
            return response.IsValidResponse;
        }
        catch
        {
            return false;
        }
    }
}
