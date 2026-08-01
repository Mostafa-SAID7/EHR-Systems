namespace EHRPlatform.Services.Terminology.Infrastructure.Services;

using EHRPlatform.Services.Terminology.Application.Services;
using EHRPlatform.Services.Terminology.Application.Features.Codes.Commands;
using EHRPlatform.Services.Terminology.Application.Features.Codes.Queries;
using EHRPlatform.Services.Terminology.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.QueryDsl;

/// <summary>
/// Elasticsearch-based code search implementation.
/// Provides full-text search and autocomplete for medical codes.
/// </summary>
public class ElasticsearchCodeSearchService : ICodeSearchService
{
    private readonly ITerminologyDbContext _context;
    private readonly ElasticsearchClient _client;
    private readonly ILogger<ElasticsearchCodeSearchService> _logger;

    public ElasticsearchCodeSearchService(
        ITerminologyDbContext context,
        ElasticsearchClient client,
        ILogger<ElasticsearchCodeSearchService> logger)
    {
        _context = context;
        _client = client;
        _logger = logger;
    }

    public async Task<CodeSearchResult> SearchCodesAsync(
        string codeSystem,
        string searchTerm,
        string? filter,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Searching {CodeSystem} for '{SearchTerm}' (page {PageNumber})", 
            codeSystem, searchTerm, pageNumber);

        try
        {
            // Query Elasticsearch index
            var indexName = $"codes-{codeSystem.ToLower()}";
            var skip = (pageNumber - 1) * pageSize;

            var response = await _client.SearchAsync<dynamic>(s => s
                .Index(indexName)
                .From(skip)
                .Size(pageSize)
                .Query(q => q
                    .MultiMatch(mm => mm
                        .Query(searchTerm)
                        .Fields(f => f.Field("code").Field("display").Field("definition")))),
                cancellationToken);

            if (!response.IsValidResponse)
            {
                _logger.LogWarning("Elasticsearch error: {Error}", response.ApiCallDetails?.OriginalException?.Message);
                return new CodeSearchResult { TotalCount = 0 };
            }

            // Map results
            var codes = new List<DiagnosisCodeDto>();
            foreach (var hit in response.Hits)
            {
                if (hit.Source is Dictionary<string, object> source)
                {
                    codes.Add(new DiagnosisCodeDto
                    {
                        Code = source.ContainsKey("code") ? source["code"].ToString() ?? "" : "",
                        Display = source.ContainsKey("display") ? source["display"].ToString() ?? "" : "",
                        Definition = source.ContainsKey("definition") ? source["definition"].ToString() : null,
                        UsageCount = source.ContainsKey("usage_count") ? Convert.ToInt32(source["usage_count"]) : 0,
                        RelevanceScore = (float)(hit.Score ?? 0)
                    });
                }
            }

            return new CodeSearchResult
            {
                Codes = codes,
                TotalCount = (int)(response.Total ?? 0)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching codes in Elasticsearch");
            throw;
        }
    }

    public async Task<List<AutocompleteCodeDto>> AutocompleteAsync(
        string codeSystem,
        string prefix,
        int maxResults,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Autocompleting {CodeSystem} with prefix '{Prefix}'", codeSystem, prefix);

        try
        {
            var indexName = $"codes-{codeSystem.ToLower()}";

            var response = await _client.SearchAsync<dynamic>(s => s
                .Index(indexName)
                .Size(maxResults)
                .Query(q => q
                    .Match(m => m
                        .Field("code.keyword")
                        .Query(prefix))),
                cancellationToken);

            var suggestions = new List<AutocompleteCodeDto>();

            foreach (var hit in response.Hits)
            {
                if (hit.Source is Dictionary<string, object> source)
                {
                    suggestions.Add(new AutocompleteCodeDto
                    {
                        Code = source.ContainsKey("code") ? source["code"].ToString() ?? "" : "",
                        Display = source.ContainsKey("display") ? source["display"].ToString() ?? "" : "",
                        UsageCount = source.ContainsKey("usage_count") ? Convert.ToInt32(source["usage_count"]) : 0
                    });
                }
            }

            return suggestions;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error autocompleting codes");
            throw;
        }
    }

    public async Task IndexCodesAsync(string codeSystem, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Indexing codes for {CodeSystem} in Elasticsearch", codeSystem);

        try
        {
            var cs = await _context.CodeSystems
                .AsNoTracking()
                .Include(c => c.Codes)
                .FirstOrDefaultAsync(c => c.Name == codeSystem, cancellationToken);

            if (cs == null)
            {
                _logger.LogWarning("Code system {CodeSystem} not found", codeSystem);
                return;
            }

            var indexName = $"codes-{codeSystem.ToLower()}";

            // Bulk index all codes
            var bulkRequest = new BulkRequest(indexName);
            foreach (var code in cs.Codes)
            {
                bulkRequest.Operations.Add(new BulkIndexOperation<dynamic>(new
                {
                    code = code.Code,
                    display = code.Display,
                    definition = code.Definition,
                    category = code.Category,
                    usage_count = code.UsageCount,
                    is_active = code.IsActive
                })
                {
                    Id = code.Id.ToString()
                });
            }

            var response = await _client.BulkAsync(bulkRequest, cancellationToken);
            _logger.LogInformation("Indexed {Count} codes for {CodeSystem}", cs.Codes.Count, codeSystem);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error indexing codes");
            throw;
        }
    }
}
