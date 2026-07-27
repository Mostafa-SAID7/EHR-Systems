using EHRPlatform.Common.CQRS;
using EHRPlatform.Services.Billing.Application.Services;

namespace EHRPlatform.Services.Billing.Application.Queries;

/// <summary>
/// Search invoices by keyword using Elasticsearch.
/// Falls back gracefully to empty results if search unavailable.
/// </summary>
public sealed record SearchInvoicesQuery(string Query, int Limit = 20) : IQuery<IEnumerable<Invoice>>;

public sealed class SearchInvoicesQueryHandler : IQueryHandler<SearchInvoicesQuery, IEnumerable<Invoice>>
{
    private readonly IBillingSearchService _searchService;
    private readonly ILogger<SearchInvoicesQueryHandler> _logger;

    public SearchInvoicesQueryHandler(IBillingSearchService searchService, ILogger<SearchInvoicesQueryHandler> logger)
    {
        _searchService = searchService;
        _logger = logger;
    }

    public async Task<IEnumerable<Invoice>> Handle(SearchInvoicesQuery query, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Executing SearchInvoicesQuery: {Query}", query.Query);

        try
        {
            var results = await _searchService.SearchInvoicesAsync(query.Query, query.Limit, cancellationToken);
            _logger.LogInformation("Found {Count} invoices matching '{Query}'", results.Count(), query.Query);
            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching invoices");
            return Enumerable.Empty<Invoice>();
        }
    }
}
