#nullable enable

using EHRPlatform.Common.Domain.Entities;
using EHRPlatform.Common.Shared.DTOs;
using EHRPlatform.Common.Domain.Enums;

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
