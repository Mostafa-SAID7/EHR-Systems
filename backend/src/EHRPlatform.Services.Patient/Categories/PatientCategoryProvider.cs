#nullable enable

using EHRPlatform.BuildingBlocks.Contracts.Contracts;
using EHRPlatform.BuildingBlocks.SharedKernel.Entities;

namespace EHRPlatform.Services.Patient.Categories;

/// <summary>
/// Category provider for Patient service.
/// Defines categorization rules, available tags, and auto-tagging logic for patients.
/// Centralizes patient-specific tagging logic to avoid duplication.
/// </summary>
public class PatientCategoryProvider : ICategoryProvider
{
    private readonly ILogger<PatientCategoryProvider> _logger;

    public string ServiceName => "Patient";
    public string ResourceType => nameof(PatientEntity);

    private static readonly Dictionary<string, string> AvailableCategoriesMap = new()
    {
        { "status", "Patient Status" },
        { "priority", "Care Priority" },
        { "classification", "Patient Classification" },
        { "workflow", "Workflow Stage" },
        { "alert", "Clinical Alert" }
    };

    public PatientCategoryProvider(ILogger<PatientCategoryProvider> logger)
    {
        _logger = logger;
    }

    public IEnumerable<string> GetAvailableCategories()
    {
        return AvailableCategoriesMap.Keys;
    }

    public async Task<IEnumerable<Tag>> GetCategoryTagsAsync(
        string categorySlug,
        ITagService tagService,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var tags = await tagService.GetByCategoryAsync(
                categorySlug,
                includeArchived: false,
                cancellationToken);

            var serviceFiltered = tags.Where(t => t.CanBeUsedByService(ServiceName));
            
            _logger.LogInformation(
                "Retrieved {Count} tags for category {Category} in {Service}",
                serviceFiltered.Count(),
                categorySlug,
                ServiceName);

            return serviceFiltered;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving tags for category {Category}", categorySlug);
            return Enumerable.Empty<Tag>();
        }
    }

    public async Task<IEnumerable<Guid>> GetRecommendedTagIdsAsync(
        Guid resourceId,
        object resourceData,
        ITagService tagService,
        CancellationToken cancellationToken = default)
    {
        var recommendedIds = new List<Guid>();

        if (resourceData is not PatientEntity patient)
        {
            _logger.LogWarning("Unexpected resource type for patient recommendations");
            return recommendedIds;
        }

        try
        {
            // Auto-recommend tags based on patient attributes

            // 1. Status-based recommendations
            if (!string.IsNullOrWhiteSpace(patient.Status))
            {
                var statusTag = await tagService.GetByNameAsync(patient.Status.ToLower(), "status", cancellationToken);
                if (statusTag != null)
                {
                    recommendedIds.Add(statusTag.Id);
                }
            }

            // 2. Age-based priority classification
            if (patient.DateOfBirth != default)
            {
                var age = DateTime.UtcNow.Year - patient.DateOfBirth.Year;
                
                string priorityTagName = age switch
                {
                    < 5 => "pediatric",        // Pediatric special care
                    >= 65 => "geriatric",      // Elderly care focus
                    _ => null                  // Adult - no age-specific tag
                };

                if (priorityTagName != null)
                {
                    var priorityTag = await tagService.GetByNameAsync(priorityTagName, "classification", cancellationToken);
                    if (priorityTag != null)
                    {
                        recommendedIds.Add(priorityTag.Id);
                    }
                }
            }

            // 3. Condition-based alert tagging
            if (patient.Conditions.Count > 0)
            {
                var chronicTag = await tagService.GetByNameAsync("chronic-condition", "alert", cancellationToken);
                if (chronicTag != null)
                {
                    recommendedIds.Add(chronicTag.Id);
                }
            }

            _logger.LogInformation(
                "Generated {Count} recommended tags for patient {PatientId}",
                recommendedIds.Count,
                resourceId);

            return recommendedIds;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating recommended tags for patient {PatientId}", resourceId);
            return recommendedIds;
        }
    }

    public async Task<bool> CanApplyTagAsync(
        Guid resourceId,
        Guid tagId,
        ITagService tagService,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var tag = await tagService.GetByIdAsync(tagId, cancellationToken);
            
            if (tag == null)
            {
                _logger.LogWarning("Tag {TagId} not found", tagId);
                return false;
            }

            // Check if tag can be used by this service
            if (!tag.CanBeUsedByService(ServiceName))
            {
                _logger.LogWarning(
                    "Tag {TagId} cannot be used by {Service}",
                    tagId,
                    ServiceName);
                return false;
            }

            // Validate category is in available list
            if (!AvailableCategoriesMap.ContainsKey(tag.Category))
            {
                _logger.LogWarning(
                    "Tag category {Category} not available for {ResourceType}",
                    tag.Category,
                    ResourceType);
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating tag {TagId}", tagId);
            return false;
        }
    }

    public CategoryMetadata GetMetadata()
    {
        return new CategoryMetadata
        {
            ServiceName = ServiceName,
            ResourceType = ResourceType,
            Description = "Centralized categorization for patient records and clinical workflows",
            AvailableCategories = AvailableCategoriesMap,
            AllowMultipleTagsPerCategory = true,
            EnableAutoTagging = true,
            MaxTagsPerResource = 20,
            IconEmoji = "👤"
        };
    }

    public async Task<IEnumerable<Guid>> GetDefaultTagsAsync(
        ITagService tagService,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var defaultTags = new List<Guid>();

            // Auto-apply "active" status tag to new patients
            var activeTag = await tagService.GetByNameAsync("active", "status", cancellationToken);
            if (activeTag != null)
            {
                defaultTags.Add(activeTag.Id);
            }

            _logger.LogInformation("Default tags for new patients: {Count}", defaultTags.Count);
            return defaultTags;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving default tags");
            return Enumerable.Empty<Guid>();
        }
    }
}

