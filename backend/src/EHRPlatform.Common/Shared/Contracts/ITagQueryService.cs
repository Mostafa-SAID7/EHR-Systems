#nullable enable

using EHRPlatform.Common.Domain.Entities;
using EHRPlatform.Common.Shared.DTOs;

namespace EHRPlatform.Common.Shared.Contracts;

/// <summary>
/// Service for querying tags with advanced filtering and search capabilities.
/// Abstraction layer supporting both in-memory and Elasticsearch backends.
/// </summary>
public interface ITagQueryService
{
    // ── Tag Filtering & Search ────────────────────────────────────────────

    /// <summary>
    /// Search tags by keyword across name, description, and slug.
    /// </summary>
    Task<IEnumerable<Tag>> SearchTagsAsync(
        string query,
        string? categoryFilter = null,
        bool includeArchived = false,
        int limit = 50,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get tags by category with optional service filtering.
    /// </summary>
    Task<IEnumerable<Tag>> GetTagsByCategoryAsync(
        string category,
        string? serviceName = null,
        bool includeArchived = false,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get tags for a specific service.
    /// </summary>
    Task<IEnumerable<Tag>> GetTagsByServiceAsync(
        string serviceName,
        string? categoryFilter = null,
        bool includeArchived = false,
        CancellationToken cancellationToken = default);

    // ── Resource Tag Queries ──────────────────────────────────────────────

    /// <summary>
    /// Get all tags applied to a resource.
    /// </summary>
    Task<IEnumerable<TagDto>> GetResourceTagsAsync(
        Guid resourceId,
        string resourceType,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Search resources by tags (multi-tag filtering with AND/OR logic).
    /// </summary>
    Task<IEnumerable<Guid>> SearchResourcesByTagsAsync(
        string resourceType,
        IEnumerable<Guid> tagIds,
        TagSearchMode mode = TagSearchMode.Any,
        string? serviceName = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get paginated resources filtered by category.
    /// </summary>
    Task<TagSearchResult> SearchResourcesAsync(
        TagSearchRequest request,
        CancellationToken cancellationToken = default);

    // ── Tag Aggregation & Analytics ────────────────────────────────────────

    /// <summary>
    /// Get tag usage statistics (count of resources per tag).
    /// </summary>
    Task<IEnumerable<TagUsageStatistic>> GetTagUsageAsync(
        string? serviceName = null,
        string? categoryFilter = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get most-used tags for a service.
    /// </summary>
    Task<IEnumerable<Tag>> GetPopularTagsAsync(
        string serviceName,
        int limit = 10,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get recently applied tags.
    /// </summary>
    Task<IEnumerable<TagWithResourceInfo>> GetRecentlyAppliedTagsAsync(
        string? serviceName = null,
        int limit = 20,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Tag search mode for multi-tag queries.
/// </summary>
public enum TagSearchMode
{
    /// <summary>Resource must have ANY of the specified tags (OR logic).</summary>
    Any = 0,

    /// <summary>Resource must have ALL of the specified tags (AND logic).</summary>
    All = 1,

    /// <summary>Resource must have EXACTLY these tags (exact match).</summary>
    Exact = 2
}

/// <summary>
/// Tag search request for advanced filtering.
/// </summary>
public record TagSearchRequest
{
    /// <summary>Resource type to search (e.g., "Patient", "Appointment").</summary>
    public required string ResourceType { get; init; }

    /// <summary>Tag IDs to filter by.</summary>
    public IEnumerable<Guid> TagIds { get; init; } = Enumerable.Empty<Guid>();

    /// <summary>Category filter for tags.</summary>
    public string? CategoryFilter { get; init; }

    /// <summary>Service name filter.</summary>
    public string? ServiceName { get; init; }

    /// <summary>Search mode (Any/All/Exact).</summary>
    public TagSearchMode Mode { get; init; } = TagSearchMode.Any;

    /// <summary>Pagination: page number (1-indexed).</summary>
    public int PageNumber { get; init; } = 1;

    /// <summary>Pagination: page size.</summary>
    public int PageSize { get; init; } = 50;

    /// <summary>Date range filter (from).</summary>
    public DateTime? TaggedFrom { get; init; }

    /// <summary>Date range filter (to).</summary>
    public DateTime? TaggedTo { get; init; }

    /// <summary>Filter by who applied the tag.</summary>
    public string? AppliedBy { get; init; }
}

/// <summary>
/// Paginated tag search results.
/// </summary>
public record TagSearchResult
{
    /// <summary>List of resource IDs matching search criteria.</summary>
    public required IEnumerable<Guid> ResourceIds { get; init; }

    /// <summary>Total count of matching resources.</summary>
    public required int TotalCount { get; init; }

    /// <summary>Current page number.</summary>
    public required int PageNumber { get; init; }

    /// <summary>Page size.</summary>
    public required int PageSize { get; init; }

    /// <summary>Total pages available.</summary>
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
}

/// <summary>
/// Tag usage statistics.
/// </summary>
public record TagUsageStatistic
{
    /// <summary>Tag ID.</summary>
    public required Guid TagId { get; init; }

    /// <summary>Tag name.</summary>
    public required string TagName { get; init; }

    /// <summary>Tag category.</summary>
    public required string Category { get; init; }

    /// <summary>Number of resources with this tag.</summary>
    public required int UsageCount { get; init; }

    /// <summary>Last applied date.</summary>
    public required DateTime? LastAppliedAt { get; init; }

    /// <summary>Percentage of total tags (0-100).</summary>
    public double UsagePercentage { get; init; }
}

/// <summary>
/// Tag with associated resource info.
/// </summary>
public record TagWithResourceInfo
{
    /// <summary>Tag ID.</summary>
    public required Guid TagId { get; init; }

    /// <summary>Tag name.</summary>
    public required string TagName { get; init; }

    /// <summary>Tag slug.</summary>
    public required string TagSlug { get; init; }

    /// <summary>Resource ID tagged with this tag.</summary>
    public required Guid ResourceId { get; init; }

    /// <summary>Resource type.</summary>
    public required string ResourceType { get; init; }

    /// <summary>Service name.</summary>
    public required string ServiceName { get; init; }

    /// <summary>When tag was applied.</summary>
    public required DateTime AppliedAt { get; init; }

    /// <summary>Who applied the tag.</summary>
    public string? AppliedBy { get; init; }
}
