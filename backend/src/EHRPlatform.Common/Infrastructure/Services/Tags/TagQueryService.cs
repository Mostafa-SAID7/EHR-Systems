#nullable enable

using Microsoft.Extensions.Logging;
using EHRPlatform.Common.Shared.Contracts;
using EHRPlatform.Common.Domain.Entities;
using EHRPlatform.Common.Shared.DTOs.Tags;
using EHRPlatform.Common.Domain.Enums;

namespace EHRPlatform.Common.Application.Features.TagManagement.Services;

/// <summary>
/// In-memory implementation of ITagQueryService.
/// MVP design: Works with in-memory tag storage. Ready for Elasticsearch migration.
/// TODO: Replace with Elasticsearch implementation for production scale.
/// </summary>
public class TagQueryService : ITagQueryService
{
    private readonly ITagService _tagService;
    private readonly ILogger<TagQueryService> _logger;

    public TagQueryService(ITagService tagService, ILogger<TagQueryService> logger)
    {
        _tagService = tagService;
        _logger = logger;
    }

    // ── Tag Filtering & Search ────────────────────────────────────────────

    public async Task<IEnumerable<Tag>> SearchTagsAsync(
        string query,
        string? categoryFilter = null,
        bool includeArchived = false,
        int limit = 50,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var allTags = await _tagService.GetAllAsync(includeArchived, categoryFilter, cancellationToken);

            var queryLower = query.ToLowerInvariant();

            var results = allTags
                .Where(t =>
                    t.Name.Contains(queryLower, StringComparison.OrdinalIgnoreCase) ||
                    t.Slug.Contains(queryLower, StringComparison.OrdinalIgnoreCase) ||
                    (t.Description?.Contains(queryLower, StringComparison.OrdinalIgnoreCase) ?? false))
                .OrderByDescending(t => t.Name.StartsWith(queryLower, StringComparison.OrdinalIgnoreCase))
                .ThenBy(t => t.Name)
                .Take(limit)
                .ToList();

            _logger.LogInformation(
                "Searched tags with query '{Query}' - found {Count} results",
                query,
                results.Count);

            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching tags for query '{Query}'", query);
            return Enumerable.Empty<Tag>();
        }
    }

    public async Task<IEnumerable<Tag>> GetTagsByCategoryAsync(
        string category,
        string? serviceName = null,
        bool includeArchived = false,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var tags = await _tagService.GetByCategoryAsync(category, includeArchived, cancellationToken);

            if (!string.IsNullOrWhiteSpace(serviceName))
            {
                tags = tags.Where(t => t.CanBeUsedByService(serviceName));
            }

            var result = tags.OrderBy(t => t.Name).ToList();

            _logger.LogInformation(
                "Retrieved {Count} tags for category {Category}" +
                (serviceName != null ? " in service {Service}" : ""),
                result.Count,
                category,
                serviceName);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving tags for category {Category}", category);
            return Enumerable.Empty<Tag>();
        }
    }

    public async Task<IEnumerable<Tag>> GetTagsByServiceAsync(
        string serviceName,
        string? categoryFilter = null,
        bool includeArchived = false,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var allTags = await _tagService.GetAllAsync(includeArchived, categoryFilter, cancellationToken);

            var results = allTags
                .Where(t => t.CanBeUsedByService(serviceName))
                .OrderBy(t => t.Category)
                .ThenBy(t => t.Name)
                .ToList();

            _logger.LogInformation(
                "Retrieved {Count} tags for service {Service}" +
                (categoryFilter != null ? " in category {Category}" : ""),
                results.Count,
                serviceName,
                categoryFilter);

            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving tags for service {Service}", serviceName);
            return Enumerable.Empty<Tag>();
        }
    }

    // ── Resource Tag Queries ──────────────────────────────────────────────

    public async Task<IEnumerable<TagDto>> GetResourceTagsAsync(
        Guid resourceId,
        string resourceType,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var tags = await _tagService.GetResourceTagsAsync(resourceId, resourceType, cancellationToken);

            var result = tags
                .Select(t => new TagDto
                {
                    Id = t.Id,
                    Name = t.Name,
                    Slug = t.Slug,
                    Category = t.Category,
                    Description = t.Description,
                    ColorCode = t.ColorCode,
                    IsSystemTag = t.IsSystemTag
                })
                .OrderBy(t => t.Category)
                .ThenBy(t => t.Name)
                .ToList();

            _logger.LogInformation(
                "Retrieved {Count} tags for {ResourceType} {ResourceId}",
                result.Count,
                resourceType,
                resourceId);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error retrieving tags for {ResourceType} {ResourceId}",
                resourceType,
                resourceId);
            return Enumerable.Empty<TagDto>();
        }
    }

    public async Task<IEnumerable<Guid>> SearchResourcesByTagsAsync(
        string resourceType,
        IEnumerable<Guid> tagIds,
        TagSearchMode mode = TagSearchMode.Any,
        string? serviceName = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var tagIdList = tagIds.ToList();

            if (!tagIdList.Any())
                return Enumerable.Empty<Guid>();

            var results = new List<Guid>();

            // For in-memory MVP, we need to fetch all associations and filter
            // In production Elasticsearch, this would be a single query
            foreach (var tagId in tagIdList)
            {
                var tagged = await _tagService.GetTaggedResourcesAsync(tagId, resourceType, cancellationToken);

                var filteredAssociations = tagged
                    .Where(ta => string.IsNullOrWhiteSpace(serviceName) || ta.ServiceName == serviceName)
                    .Select(ta => ta.ResourceId)
                    .ToList();

                if (mode == TagSearchMode.Any)
                {
                    results.AddRange(filteredAssociations);
                }
                else if (mode == TagSearchMode.All)
                {
                    if (results.Count == 0)
                        results = filteredAssociations;
                    else
                        results = results.Intersect(filteredAssociations).ToList();
                }
            }

            var uniqueResults = results.Distinct().ToList();

            _logger.LogInformation(
                "Found {Count} resources of type {ResourceType} with tags (mode: {Mode})",
                uniqueResults.Count,
                resourceType,
                mode);

            return uniqueResults;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching resources by tags");
            return Enumerable.Empty<Guid>();
        }
    }

    public async Task<TagSearchResult> SearchResourcesAsync(
        TagSearchRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var resourceIds = await SearchResourcesByTagsAsync(
                request.ResourceType,
                request.TagIds,
                request.Mode,
                request.ServiceName,
                cancellationToken);

            var resourceList = resourceIds.ToList();
            var totalCount = resourceList.Count;

            // Apply pagination
            var skip = (request.PageNumber - 1) * request.PageSize;
            var paginatedIds = resourceList
                .Skip(skip)
                .Take(request.PageSize)
                .ToList();

            _logger.LogInformation(
                "Searched resources - Page {PageNumber}/{TotalPages}, Results: {Count}/{Total}",
                request.PageNumber,
                (int)Math.Ceiling((double)totalCount / request.PageSize),
                paginatedIds.Count,
                totalCount);

            return new TagSearchResult
            {
                ResourceIds = paginatedIds,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching resources");
            return new TagSearchResult
            {
                ResourceIds = Enumerable.Empty<Guid>(),
                TotalCount = 0,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }
    }

    // ── Tag Aggregation & Analytics ────────────────────────────────────────

    public async Task<IEnumerable<TagUsageStatistic>> GetTagUsageAsync(
        string? serviceName = null,
        string? categoryFilter = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var allTags = await _tagService.GetAllAsync(false, categoryFilter, cancellationToken);

            var stats = new List<TagUsageStatistic>();
            var totalUsage = 0;

            foreach (var tag in allTags)
            {
                if (!string.IsNullOrWhiteSpace(serviceName) && !tag.CanBeUsedByService(serviceName))
                    continue;

                var usageCount = await _tagService.GetTagUsageCountAsync(tag.Id, cancellationToken);
                totalUsage += usageCount;

                stats.Add(new TagUsageStatistic
                {
                    TagId = tag.Id,
                    TagName = tag.Name,
                    Category = tag.Category,
                    UsageCount = usageCount,
                    LastAppliedAt = null, // TODO: Track in TagAssociation
                    UsagePercentage = 0 // Calculated below
                });
            }

            // Calculate percentages
            if (totalUsage > 0)
            {
                stats = stats
                    .Select(s => s with { UsagePercentage = (s.UsageCount * 100.0) / totalUsage })
                    .ToList();
            }

            var result = stats
                .OrderByDescending(s => s.UsageCount)
                .ThenBy(s => s.TagName)
                .ToList();

            _logger.LogInformation(
                "Retrieved usage stats for {Count} tags",
                result.Count);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving tag usage statistics");
            return Enumerable.Empty<TagUsageStatistic>();
        }
    }

    public async Task<IEnumerable<Tag>> GetPopularTagsAsync(
        string serviceName,
        int limit = 10,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var stats = await GetTagUsageAsync(serviceName, null, cancellationToken);

            var topTagIds = stats
                .OrderByDescending(s => s.UsageCount)
                .Take(limit)
                .Select(s => s.TagId)
                .ToList();

            var tags = new List<Tag>();
            foreach (var tagId in topTagIds)
            {
                var tag = await _tagService.GetByIdAsync(tagId, cancellationToken);
                if (tag != null)
                    tags.Add(tag);
            }

            _logger.LogInformation(
                "Retrieved {Count} popular tags for service {Service}",
                tags.Count,
                serviceName);

            return tags;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving popular tags");
            return Enumerable.Empty<Tag>();
        }
    }

    public async Task<IEnumerable<TagWithResourceInfo>> GetRecentlyAppliedTagsAsync(
        string? serviceName = null,
        int limit = 20,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // TODO: This requires enhanced TagAssociation tracking with timestamps
            // For MVP, return empty - implement when TagAssociation stores AppliedAt properly
            _logger.LogWarning("GetRecentlyAppliedTagsAsync not yet implemented for in-memory storage");
            return Enumerable.Empty<TagWithResourceInfo>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving recently applied tags");
            return Enumerable.Empty<TagWithResourceInfo>();
        }
    }
}
