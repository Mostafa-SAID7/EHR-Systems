using EHRPlatform.BuildingBlocks.EventBus.CQRS;
using EHRPlatform.Services.Analytics.Application.Services;
using EHRPlatform.Services.Analytics.Domain.Entities;

namespace EHRPlatform.Services.Analytics.Application.Queries;

/// <summary>
/// Search reports by keyword using Elasticsearch.
/// Falls back gracefully to empty results if search unavailable.
/// </summary>
public sealed record SearchReportsQuery(string Query, int Limit = 20) : IQuery<IEnumerable<Report>>;

public sealed class SearchReportsQueryHandler : IQueryHandler<SearchReportsQuery, IEnumerable<Report>>
{
    private readonly IAnalyticsSearchService _searchService;
    private readonly ILogger<SearchReportsQueryHandler> _logger;

    public SearchReportsQueryHandler(IAnalyticsSearchService searchService, ILogger<SearchReportsQueryHandler> logger)
    {
        _searchService = searchService;
        _logger = logger;
    }

    public async Task<IEnumerable<Report>> Handle(SearchReportsQuery query, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Executing SearchReportsQuery: {Query}", query.Query);

        try
        {
            var results = await _searchService.SearchReportsAsync(query.Query, query.Limit, cancellationToken);
            _logger.LogInformation("Found {Count} reports matching '{Query}'", results.Count(), query.Query);
            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching reports");
            return Enumerable.Empty<Report>();
        }
    }
}


