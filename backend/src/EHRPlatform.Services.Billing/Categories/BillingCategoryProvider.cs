#nullable enable

using EHRPlatform.Common.Shared.Contracts;
using EHRPlatform.Common.Domain.Entities;

namespace EHRPlatform.Services.Billing.Categories;

/// <summary>
/// Category provider for Billing service.
/// Defines categorization rules, available tags, and auto-tagging logic for invoices and billing records.
/// Centralizes billing-specific tagging logic to avoid duplication.
/// </summary>
public class BillingCategoryProvider : ICategoryProvider
{
    private readonly ILogger<BillingCategoryProvider> _logger;

    public string ServiceName => "Billing";
    public string ResourceType => nameof(Invoice);

    private static readonly Dictionary<string, string> AvailableCategoriesMap = new()
    {
        { "status", "Invoice Status" },
        { "priority", "Collection Priority" },
        { "payment", "Payment Method Classification" },
        { "workflow", "Billing Workflow" },
        { "alert", "Financial Alert" }
    };

    public BillingCategoryProvider(ILogger<BillingCategoryProvider> logger)
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

        if (resourceData is not Invoice invoice)
        {
            _logger.LogWarning("Unexpected resource type for invoice recommendations");
            return recommendedIds;
        }

        try
        {
            // Auto-recommend tags based on invoice attributes

            // 1. Status-based recommendations
            if (!string.IsNullOrWhiteSpace(invoice.Status))
            {
                var statusTag = await tagService.GetByNameAsync(invoice.Status.ToLower(), "status", cancellationToken);
                if (statusTag != null)
                {
                    recommendedIds.Add(statusTag.Id);
                }
            }

            // 2. Amount-based priority tagging
            if (invoice.TotalAmount > 0)
            {
                string priorityTagName = invoice.TotalAmount switch
                {
                    > 5000 => "high-value",      // High-value invoice
                    > 1000 => "medium-value",    // Medium-value invoice
                    _ => null                    // Standard value - no tag
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

            // 3. Overdue status alert tagging
            if (invoice.DueDate != default && invoice.DueDate < DateTime.UtcNow)
            {
                // Check if invoice is unpaid and overdue
                if (invoice.Status != "Paid")
                {
                    var overdueTag = await tagService.GetByNameAsync("overdue", "alert", cancellationToken);
                    if (overdueTag != null)
                    {
                        recommendedIds.Add(overdueTag.Id);
                    }
                }
            }

            _logger.LogInformation(
                "Generated {Count} recommended tags for invoice {InvoiceId}",
                recommendedIds.Count,
                resourceId);

            return recommendedIds;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating recommended tags for invoice {InvoiceId}", resourceId);
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
            Description = "Centralized categorization for invoices, billing records, and payment tracking",
            AvailableCategories = AvailableCategoriesMap,
            AllowMultipleTagsPerCategory = true,
            EnableAutoTagging = true,
            MaxTagsPerResource = 12,
            IconEmoji = "💰"
        };
    }

    public async Task<IEnumerable<Guid>> GetDefaultTagsAsync(
        ITagService tagService,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var defaultTags = new List<Guid>();

            // Auto-apply "unpaid" status tag to new invoices
            var unpaidTag = await tagService.GetByNameAsync("unpaid", "status", cancellationToken);
            if (unpaidTag != null)
            {
                defaultTags.Add(unpaidTag.Id);
            }

            _logger.LogInformation("Default tags for new invoices: {Count}", defaultTags.Count);
            return defaultTags;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving default tags");
            return Enumerable.Empty<Guid>();
        }
    }
}
