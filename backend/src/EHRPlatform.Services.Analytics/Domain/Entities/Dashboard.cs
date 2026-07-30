using EHRPlatform.Common.Domain.Entities;

namespace EHRPlatform.Services.Analytics.Domain.Entities;

public class Dashboard : BaseEntity
{
    public Guid UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsDefault { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation
    public ICollection<DashboardWidget> DashboardWidgets { get; set; } = new List<DashboardWidget>();
}

