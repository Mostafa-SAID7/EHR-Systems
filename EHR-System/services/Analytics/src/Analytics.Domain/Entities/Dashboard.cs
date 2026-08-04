namespace EHRPlatform.Services.Analytics.Domain.Entities;

/// <summary>
/// Dashboard - User-defined dashboards with widgets
/// </summary>
public class Dashboard
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsPublic { get; set; } = false;
    public int DisplayOrder { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public ICollection<DashboardWidget> Widgets { get; } = new List<DashboardWidget>();
}
