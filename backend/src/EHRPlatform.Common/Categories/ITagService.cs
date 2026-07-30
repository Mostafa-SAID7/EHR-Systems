#nullable enable

namespace EHRPlatform.Common.Tags;

/// <summary>
/// Service for managing tags across all services.
/// Provides unified tag creation, querying, and association management.
/// </summary>
public interface ITagService
{
    // ── Tag CRUD ──────────────────────────────────────────────────────────

    /// <summary>
    /// Create a new tag.
    /// </summary>
    Task<Tag> CreateAsync(
        string name,
        string category,
        string? description = null,
        string? colorCode = null,
        string? allowedServices = null,
        bool isSystemTag = false,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get tag by ID.
    /// </summary>
    Task<Tag?> GetByIdAsync(Guid tagId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get tag by slug.
    /// </summary>
    Task<Tag?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get tag by name and category (unique combination).
    /// </summary>
    Task<Tag?> GetByNameAsync(string name, string category, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update an existing tag.
    /// </summary>
    Task<Tag> UpdateAsync(
        Guid tagId,
        string? name = null,
        string? description = null,
        string? colorCode = null,
        string? allowedServices = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Archive a tag (soft delete).
    /// </summary>
    Task<bool> ArchiveAsync(Guid tagId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete a tag permanently (only if not in use).
    /// </summary>
    Task<bool> DeleteAsync(Guid tagId, CancellationToken cancellationToken = default);

    // ── Tag Querying ──────────────────────────────────────────────────────

    /// <summary>
    /// Get all tags in a category.
    /// </summary>
    Task<IEnumerable<Tag>> GetByCategoryAsync(
        string category,
        bool includeArchived = false,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Search tags by name or description.
    /// </summary>
    Task<IEnumerable<Tag>> SearchAsync(
        string query,
        string? categoryFilter = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all tags (with optional filtering).
    /// </summary>
    Task<IEnumerable<Tag>> GetAllAsync(
        bool includeArchived = false,
        string? categoryFilter = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get tags that can be used by a specific service.
    /// </summary>
    Task<IEnumerable<Tag>> GetByServiceAsync(
        string serviceName,
        string? categoryFilter = null,
        bool includeArchived = false,
        CancellationToken cancellationToken = default);

    // ── Tag Associations ──────────────────────────────────────────────────

    /// <summary>
    /// Apply a tag to a resource.
    /// </summary>
    Task<TagAssociation> ApplyTagAsync(
        Guid resourceId,
        string resourceType,
        Guid tagId,
        string serviceName,
        string? context = null,
        string? appliedBy = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Remove a tag from a resource.
    /// </summary>
    Task<bool> RemoveTagAsync(
        Guid resourceId,
        string resourceType,
        Guid tagId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all tags applied to a resource.
    /// </summary>
    Task<IEnumerable<Tag>> GetResourceTagsAsync(
        Guid resourceId,
        string resourceType,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all resources with a specific tag.
    /// </summary>
    Task<IEnumerable<TagAssociation>> GetTaggedResourcesAsync(
        Guid tagId,
        string? resourceTypeFilter = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Replace all tags on a resource.
    /// </summary>
    Task<IEnumerable<TagAssociation>> SetResourceTagsAsync(
        Guid resourceId,
        string resourceType,
        IEnumerable<Guid> tagIds,
        string serviceName,
        string? appliedBy = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Bulk apply a tag to multiple resources.
    /// </summary>
    Task<int> BulkApplyTagAsync(
        IEnumerable<Guid> resourceIds,
        string resourceType,
        Guid tagId,
        string serviceName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if a resource has a specific tag.
    /// </summary>
    Task<bool> HasTagAsync(
        Guid resourceId,
        string resourceType,
        Guid tagId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get resource count for a tag.
    /// </summary>
    Task<int> GetTagUsageCountAsync(Guid tagId, CancellationToken cancellationToken = default);
}
