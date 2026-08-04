namespace EHRPlatform.Services.Analytics.Domain.Services;

using EHRPlatform.Services.Analytics.Domain.Entities;
using EHRPlatform.Services.Analytics.Domain.Enums;

/// <summary>
/// Factory service for creating Dashboard aggregates
/// Ensures consistent creation and validation
/// </summary>
public class DashboardFactory
{
    /// <summary>
    /// Creates new dashboard with validation
    /// </summary>
    public Dashboard CreateDashboard(
        string name,
        string description,
        Guid createdBy,
        long tenantId,
        DashboardVisibility visibility = DashboardVisibility.Private)
    {
        ValidateDashboardInput(name, description);

        return new Dashboard
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = description,
            CreatedBy = createdBy,
            TenantId = tenantId,
            IsPublic = visibility == DashboardVisibility.Organization,
            DisplayOrder = 0,
            CreatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Creates dashboard from existing data (for loading from repository)
    /// </summary>
    public Dashboard LoadDashboard(
        Guid id,
        string name,
        string description,
        Guid createdBy,
        long tenantId,
        bool isPublic,
        int displayOrder,
        DateTime createdAt,
        DateTime? updatedAt = null)
    {
        return new Dashboard
        {
            Id = id,
            Name = name,
            Description = description,
            CreatedBy = createdBy,
            TenantId = tenantId,
            IsPublic = isPublic,
            DisplayOrder = displayOrder,
            CreatedAt = createdAt,
            UpdatedAt = updatedAt
        };
    }

    /// <summary>
    /// Validates dashboard input parameters
    /// </summary>
    private void ValidateDashboardInput(string name, string description)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Dashboard name is required", nameof(name));

        if (name.Length > 200)
            throw new ArgumentException("Dashboard name cannot exceed 200 characters", nameof(name));

        if (description.Length > 1000)
            throw new ArgumentException("Dashboard description cannot exceed 1000 characters", nameof(description));
    }
}
