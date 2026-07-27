#nullable enable

using EHRPlatform.Common.Categories;
using EHRPlatform.Common.Tags;

namespace EHRPlatform.Services.Appointment.Categories;

/// <summary>
/// Category provider for Appointment service.
/// Defines categorization rules, available tags, and auto-tagging logic for appointments.
/// Centralizes appointment-specific tagging logic to avoid duplication.
/// </summary>
public class AppointmentCategoryProvider : ICategoryProvider
{
    private readonly ILogger<AppointmentCategoryProvider> _logger;

    public string ServiceName => "Appointment";
    public string ResourceType => nameof(Appointment);

    private static readonly Dictionary<string, string> AvailableCategoriesMap = new()
    {
        { "status", "Appointment Status" },
        { "priority", "Appointment Priority" },
        { "type", "Appointment Type Classification" },
        { "workflow", "Appointment Workflow" },
        { "alert", "Clinical Alert" }
    };

    public AppointmentCategoryProvider(ILogger<AppointmentCategoryProvider> logger)
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

        // Use fully-qualified name to avoid namespace conflict
        if (resourceData is not global::EHRPlatform.Services.Appointment.Features.Appointments.Domain.Appointment appointment)
        {
            _logger.LogWarning("Unexpected resource type for appointment recommendations");
            return recommendedIds;
        }

        try
        {
            // Auto-recommend tags based on appointment attributes

            // 1. Status-based recommendations
            if (!string.IsNullOrWhiteSpace(appointment.Status))
            {
                var statusTag = await tagService.GetByNameAsync(appointment.Status.ToLower(), "status", cancellationToken);
                if (statusTag != null)
                {
                    recommendedIds.Add(statusTag.Id);
                }
            }

            // 2. Appointment type recommendations
            if (!string.IsNullOrWhiteSpace(appointment.AppointmentType))
            {
                // Look for a tag matching the appointment type
                var typeTag = await tagService.GetByNameAsync(
                    appointment.AppointmentType.ToLower(),
                    "type",
                    cancellationToken);
                
                if (typeTag != null)
                {
                    recommendedIds.Add(typeTag.Id);
                }
            }

            // 3. Time-based priority tagging
            if (appointment.ScheduledStart != default)
            {
                var now = DateTime.UtcNow;
                var daysUntilAppointment = (appointment.ScheduledStart - now).TotalDays;

                string priorityTagName = daysUntilAppointment switch
                {
                    < 1 => "urgent",           // Appointment is today or overdue
                    < 3 => "high",             // Within 3 days
                    < 7 => "medium",           // Within a week
                    _ => null                  // Normal priority - no tag
                };

                if (priorityTagName != null)
                {
                    var priorityTag = await tagService.GetByNameAsync(priorityTagName, "priority", cancellationToken);
                    if (priorityTag != null)
                    {
                        recommendedIds.Add(priorityTag.Id);
                    }
                }
            }

            _logger.LogInformation(
                "Generated {Count} recommended tags for appointment {AppointmentId}",
                recommendedIds.Count,
                resourceId);

            return recommendedIds;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating recommended tags for appointment {AppointmentId}", resourceId);
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
            Description = "Centralized categorization for scheduled appointments and clinical encounters",
            AvailableCategories = AvailableCategoriesMap,
            AllowMultipleTagsPerCategory = true,
            EnableAutoTagging = true,
            MaxTagsPerResource = 15,
            IconEmoji = "📅"
        };
    }

    public async Task<IEnumerable<Guid>> GetDefaultTagsAsync(
        ITagService tagService,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var defaultTags = new List<Guid>();

            // Auto-apply "scheduled" status tag to new appointments
            var scheduledTag = await tagService.GetByNameAsync("scheduled", "status", cancellationToken);
            if (scheduledTag != null)
            {
                defaultTags.Add(scheduledTag.Id);
            }

            _logger.LogInformation("Default tags for new appointments: {Count}", defaultTags.Count);
            return defaultTags;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving default tags");
            return Enumerable.Empty<Guid>();
        }
    }
}
