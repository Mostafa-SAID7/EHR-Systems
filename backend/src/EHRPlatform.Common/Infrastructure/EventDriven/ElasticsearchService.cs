using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Analysis;
using Elastic.Clients.Elasticsearch.Core.Search;
using Elastic.Clients.Elasticsearch.IndexManagement;
using Elastic.Clients.Elasticsearch.QueryDsl;
using EHRPlatform.Common.Shared.Utilities;

namespace EHRPlatform.Common.Search;

/// <summary>
/// Elasticsearch implementation of <see cref="ISearchService"/>.
/// Provides full-text search with optional medical-terminology analyzer support.
/// </summary>
public sealed class ElasticsearchService : ISearchService
{
    private readonly ElasticsearchClient _client;

    public ElasticsearchService(ElasticsearchClient client)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
    }

    // ─── ISearchService implementation ───────────────────────────────────────

    public async Task<SearchResult<T>> SearchAsync<T>(
        SearchQuery query,
        CancellationToken cancellationToken = default) where T : class
    {
        ArgumentGuard.NotNull(query, nameof(query));

        var indexName = GetIndexName<T>();
        var pageSize  = Math.Min(query.PageSize, 100);
        var from      = (query.PageNumber - 1) * pageSize;

        try
        {
            var request = new SearchRequest(indexName)
            {
                From  = from,
                Size  = pageSize,
                Query = BuildQuery(query)
            };

            if (query.HighlightResults)
            {
                request.Highlight = new Highlight
                {
                    Fields = new Dictionary<Field, HighlightField>
                    {
                        { "*", new HighlightField() }
                    }
                };
            }

            var response = await _client.SearchAsync<T>(request, cancellationToken);

            if (!response.IsSuccess())
                throw new SearchException(
                    $"Search failed: {response.ApiCallDetails?.OriginalException?.Message}");

            return MapSearchResult(response, query.PageNumber, pageSize);
        }
        catch (SearchException) { throw; }
        catch (Exception ex)
        {
            throw new SearchException($"Search error for {typeof(T).Name}: {ex.Message}", ex);
        }
    }

    public async Task IndexAsync<T>(
        string id,
        T entity,
        CancellationToken cancellationToken = default) where T : class
    {
        ArgumentGuard.NotNullOrEmpty(id, nameof(id));
        ArgumentGuard.NotNull(entity, nameof(entity));

        var request  = new IndexRequest<T>(entity, GetIndexName<T>(), id: id);
        var response = await _client.IndexAsync(request, cancellationToken);

        if (!response.IsSuccess())
            throw new SearchException($"Index failed: {response.ApiCallDetails?.OriginalException?.Message}");
    }

    public async Task IndexBulkAsync<T>(
        IEnumerable<(string id, T entity)> items,
        CancellationToken cancellationToken = default) where T : class
    {
        ArgumentGuard.NotNull(items, nameof(items));

        var list = items.ToList();
        if (list.Count == 0) return;

        // Individual index calls: avoids generic-constraint issues with the v8 bulk descriptor API.
        var tasks = list.Select(item => IndexAsync(item.id, item.entity, cancellationToken));
        await Task.WhenAll(tasks);
    }

    public async Task DeleteAsync<T>(
        string id,
        CancellationToken cancellationToken = default) where T : class
    {
        ArgumentGuard.NotNullOrEmpty(id, nameof(id));

        var request  = new DeleteRequest(GetIndexName<T>(), id);
        var response = await _client.DeleteAsync(request, cancellationToken);

        if (!response.IsSuccess())
            throw new SearchException($"Delete failed: {response.ApiCallDetails?.OriginalException?.Message}");
    }

    public async Task RebuildIndexAsync<T>(CancellationToken cancellationToken = default) where T : class
    {
        if (await IndexExistsAsync<T>(cancellationToken))
            await _client.Indices.DeleteAsync(GetIndexName<T>(), cancellationToken: cancellationToken);

        await CreateIndexAsync<T>(cancellationToken);
    }

    public async Task<bool> IndexExistsAsync<T>(CancellationToken cancellationToken = default) where T : class
    {
        try
        {
            var r = await _client.Indices.ExistsAsync(GetIndexName<T>(), cancellationToken: cancellationToken);
            return r.Exists;
        }
        catch { return false; }
    }

    public async Task DeleteIndexAsync<T>(CancellationToken cancellationToken = default) where T : class
    {
        var r = await _client.Indices.DeleteAsync(GetIndexName<T>(), cancellationToken: cancellationToken);

        if (!r.IsSuccess())
            throw new SearchException($"Delete index failed: {r.ApiCallDetails?.OriginalException?.Message}");
    }

    public async Task<SearchStatistics> GetStatisticsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var r     = await _client.Indices.GetMappingAsync(new GetMappingRequest(), cancellationToken);
            var stats = new SearchStatistics();

            if (r.Indices != null)
                foreach (var (indexName, _) in r.Indices)
                    stats.IndexDocumentCounts[indexName.ToString()] = 0;

            return stats;
        }
        catch (Exception ex)
        {
            throw new SearchException($"Statistics error: {ex.Message}", ex);
        }
    }

    // ─── Helpers ─────────────────────────────────────────────────────────────

    private static string GetIndexName<T>() where T : class =>
        typeof(T).Name.ToLowerInvariant() + "-index";

    private static Query BuildQuery(SearchQuery query)
    {
        var filters = new List<Query>();

        if (!string.IsNullOrWhiteSpace(query.QueryText))
        {
            filters.Add(new MultiMatchQuery
            {
                Query  = query.QueryText,
                Fields = new[] { "*" }
            });
        }

        if (query.FieldFilters?.Any() == true)
        {
            foreach (var (field, value) in query.FieldFilters)
                filters.Add(new TermQuery(new Field(field)) { Value = FieldValue.String(value) });
        }

        if (query.DateRange.HasValue)
        {
            var (from, to) = query.DateRange.Value;
            var drq = new DateRangeQuery(new Field("createdAt"));
            if (from.HasValue) drq.Gte = from.Value;
            if (to.HasValue)   drq.Lte = to.Value;
            filters.Add(drq);
        }

        return filters.Count switch
        {
            0 => new MatchAllQuery(),
            1 => filters[0],
            _ => new BoolQuery { Must = filters }
        };
    }

    private static SearchResult<T> MapSearchResult<T>(
        Elastic.Clients.Elasticsearch.SearchResponse<T> response,
        int pageNumber,
        int pageSize) where T : class
    {
        return new SearchResult<T>
        {
            TotalCount = response.Total,
            PageNumber = pageNumber,
            PageSize   = pageSize,
            Hits = response.Hits
                ?.Select(h => new SearchHit<T>
                {
                    Id         = h.Id ?? string.Empty,
                    Document   = h.Source!,
                    Score      = h.Score ?? 0,
                    Highlights = h.Highlight?.ToDictionary(
                        kv => kv.Key,
                        kv => kv.Value.ToArray())
                })
                .ToList() ?? new List<SearchHit<T>>()
        };
    }

    private async Task CreateIndexAsync<T>(CancellationToken cancellationToken) where T : class
    {
        var request = new CreateIndexRequest(GetIndexName<T>())
        {
            Settings = new IndexSettings
            {
                Analysis = new IndexSettingsAnalysis
                {
                    Analyzers = new Analyzers
                    {
                        ["medical_analyzer"] = new CustomAnalyzer
                        {
                            Tokenizer = "standard",
                            Filter    = new[] { "lowercase", "medical_synonyms" }
                        }
                    },
                    TokenFilters = new TokenFilters
                    {
                        ["medical_synonyms"] = new SynonymTokenFilter
                        {
                            Synonyms = new[]
                            {
                                "diabetes => dm",
                                "hypertension => htn",
                                "congestive heart failure => chf",
                                "myocardial infarction => mi"
                            }
                        }
                    }
                }
            }
        };

        await _client.Indices.CreateAsync(request, cancellationToken);
    }
}

/// <summary>Search-specific exception.</summary>
public sealed class SearchException : Exception
{
    public SearchException(string message) : base(message) { }
    public SearchException(string message, Exception inner) : base(message, inner) { }
}

