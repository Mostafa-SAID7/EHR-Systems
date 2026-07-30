#nullable enable

namespace EHRPlatform.Common.Domain.Entities;

/// <summary>
/// Tag entity for categorizing and labeling resources across all services.
/// Supports cross-service tagging for flexible classification.
/// Single Responsibility: Represent a reusable tag/label in the system.
/// </summary>
public class Tag : AuditableEntity
{
    /// <summary>
    /// Tag name (e.g., "urgent", "follow-up", "reviewed").
    /// Must be unique within a category.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// URL-friendly slug of tag name (auto-generated).
    /// Used for URL-based tag queries.
    /// </summary>
    public string Slug { get; set; } = string.Empty;

    /// <summary>
    /// Tag category/type (e.g., "workflow", "priority", "status", "classification").
    /// Enables grouping tags by purpose.
    /// </summary>
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// Human-readable description of what this tag represents.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Color code for UI display (hex format, e.g., "#FF5733").
    /// Helps visually distinguish tags in interfaces.
    /// </summary>
    public string? ColorCode { get; set; }

    /// <summary>
    /// Whether tag is archived (soft delete).
    /// Archived tags cannot be applied to new resources but remain on existing ones.
    /// </summary>
    public bool IsArchived { get; set; }

    /// <summary>
    /// Usage count tracking (denormalized for performance).
    /// Updated whenever tag is applied/removed.
    /// </summary>
    public int UsageCount { get; set; }

    /// <summary>
    /// Whether tag is system-managed (read-only).
    /// System tags cannot be modified or deleted by users.
    /// </summary>
    public bool IsSystemTag { get; set; }

    /// <summary>
    /// Optional: Define which services can use this tag.
    /// Comma-separated list of service names, or null for all services.
    /// Example: "Patient,Appointment,Clinical"
    /// </summary>
    public string? AllowedServices { get; set; }

    /// <summary>
    /// Get display value for tag (name or description).
    /// </summary>
    public string GetDisplayValue() => !string.IsNullOrWhiteSpace(Description) ? Description : Name;

    /// <summary>
    /// Check if tag can be used by a specific service.
    /// </summary>
    public bool CanBeUsedByService(string serviceName)
    {
        if (IsArchived)
            return false;

        if (string.IsNullOrWhiteSpace(AllowedServices))
            return true; // No restrictions

        var allowed = AllowedServices.Split(',', StringSplitOptions.TrimEntries);
        return allowed.Contains(serviceName, StringComparer.OrdinalIgnoreCase);
    }
}

/// <summary>
/// Tag association: links a tag to a specific resource.
/// Supports tagging any entity across all services.
/// </summary>
public class TagAssociation : BaseEntity
{
    /// <summary>
    /// ID of the tag being applied.
    /// </summary>
    public Guid TagId { get; set; }

    /// <summary>
    /// Navigation property to tag.
    /// </summary>
    public virtual Tag? Tag { get; set; }

    /// <summary>
    /// ID of the resource being tagged (e.g., Patient, Appointment, Invoice).
    /// </summary>
    public Guid ResourceId { get; set; }

    /// <summary>
    /// Type/class name of the resource being tagged.
    /// Examples: "Patient", "Appointment", "Invoice", "ClinicalNote"
    /// </summary>
    public string ResourceType { get; set; } = string.Empty;

    /// <summary>
    /// Optional context/relationship description.
    /// Can store additional metadata about why this tag applies.
    /// </summary>
    public string? Context { get; set; }

    /// <summary>
    /// Tenant/service identifier for multi-tenant scenarios.
    /// Ensures tag associations are scoped to correct service.
    /// </summary>
    public string ServiceName { get; set; } = string.Empty;

    /// <summary>
    /// When this tag was applied to the resource.
    /// </summary>
    public DateTime AppliedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Who applied the tag (user ID or system identifier).
    /// </summary>
    public string? AppliedBy { get; set; }

    /// <summary>
    /// Get composite key for efficient uniqueness checking.
    /// </summary>
    public string GetCompositeKey() => $"{TagId}_{ResourceId}_{ResourceType}";
}
