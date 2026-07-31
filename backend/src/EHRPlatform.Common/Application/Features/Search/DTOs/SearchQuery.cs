#nullable enable

using EHRPlatform.Common.Domain.Enums;

namespace EHRPlatform.Common.Application.Features.Search.Services;

/// <summary>
/// Search query for Elasticsearch.
/// Supports full-text search, filters, sorting, and pagination.
/// </summary>
public class SearchQuery
{
    /// <summary>
    /// Query text for full-text search.
    /// Example: "diabetes patient" searches across all indexed fields.
    /// </summary>
    public string? QueryText { get; set; }

    /// <summary>
    /// Field-specific search.
    /// Example: ("FirstName", "John") searches only FirstName field.
    /// </summary>
    public Dictionary<string, string>? FieldFilters { get; set; }

    /// <summary>
    /// Date range filter.
    /// </summary>
    public (DateTime? From, DateTime? To)? DateRange { get; set; }

    /// <summary>
    /// Pagination - page number (1-based).
    /// </summary>
    public int PageNumber { get; set; } = 1;

    /// <summary>
    /// Pagination - items per page (max 100).
    /// </summary>
    public int PageSize { get; set; } = 10;

    /// <summary>
    /// Fields to sort by.
    /// Example: ("CreatedAt", SortOrder.Descending)
    /// </summary>
    public List<(string field, SortOrder order)>? SortBy { get; set; }

    /// <summary>
    /// Highlight search results in response.
    /// </summary>
    public bool HighlightResults { get; set; } = true;

    /// <summary>
    /// Request facets (aggregations).
    /// Example: ["Status", "Department"] - returns counts per value.
    /// </summary>
    public List<string>? Facets { get; set; }
}
