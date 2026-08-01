#nullable enable

using EHRPlatform.Common.Slugs;
using Microsoft.Extensions.Logging;
using EHRPlatform.Common.Shared.Contracts;
using EHRPlatform.Common.Domain.Entities;
using EHRPlatform.Common.Shared.Utilities.Helpers;

namespace EHRPlatform.Common.Application.Features.TagManagement.Services;

/// <summary>
/// In-memory tag service implementation.
/// Provides basic tag management without database persistence (for MVP).
/// Production implementation should use EF Core + database storage.
/// </summary>
public class TagService : ITagService
{
    private readonly ISlugGenerator _slugGenerator;
    private readonly ILogger<TagService> _logger;

    // In-memory storage (for MVP)
    private readonly Dictionary<Guid, Tag> _tags = new();
    private readonly List<TagAssociation> _associations = new();
    private readonly object _lockObj = new();

    public TagService(ISlugGenerator slugGenerator, ILogger<TagService> logger)
    {
        _slugGenerator = slugGenerator ?? throw new ArgumentNullException(nameof(slugGenerator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        // Seed common tags
        SeedCommonTags();
    }

    public Task<Tag> CreateAsync(
        string name,
        string category,
        string? description = null,
        string? colorCode = null,
        string? allowedServices = null,
        bool isSystemTag = false,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Tag name cannot be empty", nameof(name));
        if (string.IsNullOrWhiteSpace(category))
            throw new ArgumentException("Tag category cannot be empty", nameof(category));

        var slug = _slugGenerator.Generate(name);

        lock (_lockObj)
        {
            var existing = _tags.Values.FirstOrDefault(t => t.Slug == slug && t.Category == category);
            if (existing != null)
                throw new InvalidOperationException($"Tag '{name}' already exists in category '{category}'");

            var tag = new Tag
            {
                Id = GuidHelper.NewGuid(),
                Name = name,
                Slug = slug,
                Category = category,
                Description = description,
                ColorCode = colorCode,
                AllowedServices = allowedServices,
                IsSystemTag = isSystemTag,
                CreatedAt = DateTimeHelper.UtcNow
            };

            _tags[tag.Id] = tag;
            _logger.LogInformation("Created tag {TagId}: {TagName}", tag.Id, tag.Name);

            return Task.FromResult(tag);
        }
    }

    public Task<Tag?> GetByIdAsync(Guid tagId, CancellationToken cancellationToken = default)
    {
        lock (_lockObj)
        {
            _tags.TryGetValue(tagId, out var tag);
            return Task.FromResult(tag);
        }
    }

    public Task<Tag?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        lock (_lockObj)
        {
            var tag = _tags.Values.FirstOrDefault(t => t.Slug == slug && !t.IsArchived);
            return Task.FromResult(tag);
        }
    }

    public Task<Tag?> GetByNameAsync(string name, string category, CancellationToken cancellationToken = default)
    {
        lock (_lockObj)
        {
            var tag = _tags.Values.FirstOrDefault(t => 
                t.Name == name && t.Category == category && !t.IsArchived);
            return Task.FromResult(tag);
        }
    }

    public Task<Tag> UpdateAsync(
        Guid tagId,
        string? name = null,
        string? description = null,
        string? colorCode = null,
        string? allowedServices = null,
        CancellationToken cancellationToken = default)
    {
        lock (_lockObj)
        {
            if (!_tags.TryGetValue(tagId, out var tag))
                throw new InvalidOperationException($"Tag {tagId} not found");

            if (!string.IsNullOrWhiteSpace(name) && name != tag.Name)
            {
                tag.Name = name;
                tag.Slug = _slugGenerator.Generate(name);
            }

            if (description != null)
                tag.Description = description;

            if (colorCode != null)
                tag.ColorCode = colorCode;

            if (allowedServices != null)
                tag.AllowedServices = allowedServices;

            tag.UpdatedAt = DateTimeHelper.UtcNow;
            _logger.LogInformation("Updated tag {TagId}", tagId);

            return Task.FromResult(tag);
        }
    }

    public Task<bool> ArchiveAsync(Guid tagId, CancellationToken cancellationToken = default)
    {
        lock (_lockObj)
        {
            if (!_tags.TryGetValue(tagId, out var tag))
                return Task.FromResult(false);

            tag.IsArchived = true;
            tag.UpdatedAt = DateTimeHelper.UtcNow;
            _logger.LogInformation("Archived tag {TagId}", tagId);

            return Task.FromResult(true);
        }
    }

    public Task<bool> DeleteAsync(Guid tagId, CancellationToken cancellationToken = default)
    {
        lock (_lockObj)
        {
            var associations = _associations.Count(a => a.TagId == tagId);
            if (associations > 0)
                throw new InvalidOperationException($"Cannot delete tag {tagId}: {associations} associations exist");

            return Task.FromResult(_tags.Remove(tagId));
        }
    }

    public Task<IEnumerable<Tag>> GetByCategoryAsync(
        string category,
        bool includeArchived = false,
        CancellationToken cancellationToken = default)
    {
        lock (_lockObj)
        {
            var tags = _tags.Values
                .Where(t => t.Category == category && (includeArchived || !t.IsArchived))
                .ToList();
            return Task.FromResult((IEnumerable<Tag>)tags);
        }
    }

    public Task<IEnumerable<Tag>> SearchAsync(
        string query,
        string? categoryFilter = null,
        CancellationToken cancellationToken = default)
    {
        lock (_lockObj)
        {
            var searchLower = query.ToLower();
            var tags = _tags.Values
                .Where(t => !t.IsArchived &&
                    (t.Name.Contains(searchLower, StringComparison.OrdinalIgnoreCase) ||
                     (t.Description?.Contains(searchLower, StringComparison.OrdinalIgnoreCase) ?? false)) &&
                    (categoryFilter == null || t.Category == categoryFilter))
                .ToList();
            return Task.FromResult((IEnumerable<Tag>)tags);
        }
    }

    public Task<IEnumerable<Tag>> GetAllAsync(
        bool includeArchived = false,
        string? categoryFilter = null,
        CancellationToken cancellationToken = default)
    {
        lock (_lockObj)
        {
            var tags = _tags.Values
                .Where(t => (includeArchived || !t.IsArchived) &&
                    (categoryFilter == null || t.Category == categoryFilter))
                .ToList();
            return Task.FromResult((IEnumerable<Tag>)tags);
        }
    }

    public Task<IEnumerable<Tag>> GetByServiceAsync(
        string serviceName,
        string? categoryFilter = null,
        bool includeArchived = false,
        CancellationToken cancellationToken = default)
    {
        lock (_lockObj)
        {
            var tags = _tags.Values
                .Where(t => !t.IsArchived &&
                    t.CanBeUsedByService(serviceName) &&
                    (categoryFilter == null || t.Category == categoryFilter))
                .ToList();
            return Task.FromResult((IEnumerable<Tag>)tags);
        }
    }

    public Task<TagAssociation> ApplyTagAsync(
        Guid resourceId,
        string resourceType,
        Guid tagId,
        string serviceName,
        string? context = null,
        string? appliedBy = null,
        CancellationToken cancellationToken = default)
    {
        lock (_lockObj)
        {
            if (!_tags.TryGetValue(tagId, out var tag))
                throw new InvalidOperationException($"Tag {tagId} not found");

            if (!tag.CanBeUsedByService(serviceName))
                throw new InvalidOperationException($"Tag {tagId} cannot be used by service {serviceName}");

            var existing = _associations.FirstOrDefault(a =>
                a.ResourceId == resourceId &&
                a.ResourceType == resourceType &&
                a.TagId == tagId);

            if (existing != null)
                return Task.FromResult(existing);

            var association = new TagAssociation
            {
                Id = GuidHelper.NewGuid(),
                TagId = tagId,
                ResourceId = resourceId,
                ResourceType = resourceType,
                Context = context,
                ServiceName = serviceName,
                AppliedBy = appliedBy,
                AppliedAt = DateTimeHelper.UtcNow
            };

            _associations.Add(association);
            tag.UsageCount++;

            _logger.LogInformation("Applied tag {TagId} to {ResourceType} {ResourceId}",
                tagId, resourceType, resourceId);

            return Task.FromResult(association);
        }
    }

    public Task<bool> RemoveTagAsync(
        Guid resourceId,
        string resourceType,
        Guid tagId,
        CancellationToken cancellationToken = default)
    {
        lock (_lockObj)
        {
            var index = _associations.FindIndex(a =>
                a.ResourceId == resourceId &&
                a.ResourceType == resourceType &&
                a.TagId == tagId);

            if (index < 0)
                return Task.FromResult(false);

            _associations.RemoveAt(index);

            if (_tags.TryGetValue(tagId, out var tag))
                tag.UsageCount = Math.Max(0, tag.UsageCount - 1);

            return Task.FromResult(true);
        }
    }

    public Task<IEnumerable<Tag>> GetResourceTagsAsync(
        Guid resourceId,
        string resourceType,
        CancellationToken cancellationToken = default)
    {
        lock (_lockObj)
        {
            var tags = _associations
                .Where(a => a.ResourceId == resourceId && a.ResourceType == resourceType)
                .Select(a => _tags.TryGetValue(a.TagId, out var tag) ? tag : null)
                .Where(t => t != null && !t.IsArchived)
                .Cast<Tag>()
                .ToList();
            return Task.FromResult((IEnumerable<Tag>)tags);
        }
    }

    public Task<IEnumerable<TagAssociation>> GetTaggedResourcesAsync(
        Guid tagId,
        string? resourceTypeFilter = null,
        CancellationToken cancellationToken = default)
    {
        lock (_lockObj)
        {
            var associations = _associations
                .Where(a => a.TagId == tagId &&
                    (resourceTypeFilter == null || a.ResourceType == resourceTypeFilter))
                .ToList();
            return Task.FromResult((IEnumerable<TagAssociation>)associations);
        }
    }

    public Task<IEnumerable<TagAssociation>> SetResourceTagsAsync(
        Guid resourceId,
        string resourceType,
        IEnumerable<Guid> tagIds,
        string serviceName,
        string? appliedBy = null,
        CancellationToken cancellationToken = default)
    {
        lock (_lockObj)
        {
            var existing = _associations
                .Where(a => a.ResourceId == resourceId && a.ResourceType == resourceType)
                .ToList();

            foreach (var assoc in existing)
            {
                _associations.Remove(assoc);
                if (_tags.TryGetValue(assoc.TagId, out var tag))
                    tag.UsageCount = Math.Max(0, tag.UsageCount - 1);
            }

            var newAssociations = new List<TagAssociation>();
            foreach (var tagId in tagIds)
            {
                if (_tags.TryGetValue(tagId, out var tag))
                {
                    var assoc = new TagAssociation
                    {
                        Id = GuidHelper.NewGuid(),
                        TagId = tagId,
                        ResourceId = resourceId,
                        ResourceType = resourceType,
                        ServiceName = serviceName,
                        AppliedBy = appliedBy,
                        AppliedAt = DateTimeHelper.UtcNow
                    };
                    _associations.Add(assoc);
                    tag.UsageCount++;
                    newAssociations.Add(assoc);
                }
            }

            return Task.FromResult((IEnumerable<TagAssociation>)newAssociations);
        }
    }

    public Task<int> BulkApplyTagAsync(
        IEnumerable<Guid> resourceIds,
        string resourceType,
        Guid tagId,
        string serviceName,
        CancellationToken cancellationToken = default)
    {
        int count = 0;
        lock (_lockObj)
        {
            if (!_tags.TryGetValue(tagId, out var tag))
                throw new InvalidOperationException($"Tag {tagId} not found");

            foreach (var resourceId in resourceIds)
            {
                var existing = _associations.FirstOrDefault(a =>
                    a.ResourceId == resourceId &&
                    a.ResourceType == resourceType &&
                    a.TagId == tagId);

                if (existing == null)
                {
                    _associations.Add(new TagAssociation
                    {
                        Id = GuidHelper.NewGuid(),
                        TagId = tagId,
                        ResourceId = resourceId,
                        ResourceType = resourceType,
                        ServiceName = serviceName,
                        AppliedAt = DateTimeHelper.UtcNow
                    });
                    tag.UsageCount++;
                    count++;
                }
            }
        }
        return Task.FromResult(count);
    }

    public Task<bool> HasTagAsync(
        Guid resourceId,
        string resourceType,
        Guid tagId,
        CancellationToken cancellationToken = default)
    {
        lock (_lockObj)
        {
            var hasTag = _associations.Any(a =>
                a.ResourceId == resourceId &&
                a.ResourceType == resourceType &&
                a.TagId == tagId);
            return Task.FromResult(hasTag);
        }
    }

    public Task<int> GetTagUsageCountAsync(Guid tagId, CancellationToken cancellationToken = default)
    {
        lock (_lockObj)
        {
            if (_tags.TryGetValue(tagId, out var tag))
                return Task.FromResult(tag.UsageCount);
            return Task.FromResult(0);
        }
    }

    private void SeedCommonTags()
    {
        var commonTags = new[]
        {
            new { Name = "Urgent", Category = "Priority", Color = "#FF0000" },
            new { Name = "High", Category = "Priority", Color = "#FF6600" },
            new { Name = "Medium", Category = "Priority", Color = "#FFCC00" },
            new { Name = "Low", Category = "Priority", Color = "#00CC00" },
            new { Name = "Follow-up", Category = "Workflow", Color = "#0099FF" },
            new { Name = "Reviewed", Category = "Workflow", Color = "#9900FF" },
            new { Name = "Pending", Category = "Workflow", Color = "#FF9900" },
            new { Name = "Completed", Category = "Workflow", Color = "#00FF00" },
        };

        lock (_lockObj)
        {
            foreach (var tagInfo in commonTags)
            {
                var slug = _slugGenerator.Generate(tagInfo.Name);
                var tag = new Tag
                {
                    Id = GuidHelper.NewGuid(),
                    Name = tagInfo.Name,
                    Slug = slug,
                    Category = tagInfo.Category,
                    ColorCode = tagInfo.Color,
                    IsSystemTag = true,
                    CreatedAt = DateTimeHelper.UtcNow
                };
                _tags[tag.Id] = tag;
            }
        }
    }
}
