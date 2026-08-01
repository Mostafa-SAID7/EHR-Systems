namespace EHRPlatform.Services.Terminology.Application.Services;

using EHRPlatform.Services.Terminology.Application.Features.Codes.Commands;

/// <summary>
/// Interface for Elasticsearch-based code search service.
/// Provides full-text search, autocomplete, and filtering.
/// </summary>
public interface ICodeSearchService
{
    /// <summary>
    /// Full-text search for codes in a code system.
    /// </summary>
    Task<CodeSearchResult> SearchCodesAsync(
        string codeSystem,
        string searchTerm,
        string? filter,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Autocomplete suggestions as user types.
    /// </summary>
    Task<List<AutocompleteCodeDto>> AutocompleteAsync(
        string codeSystem,
        string prefix,
        int maxResults,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Index codes in Elasticsearch for search.
    /// </summary>
    Task IndexCodesAsync(string codeSystem, CancellationToken cancellationToken = default);
}

public class CodeSearchResult
{
    public List<DiagnosisCodeDto> Codes { get; set; } = new();
    public int TotalCount { get; set; }
}
