namespace EHRPlatform.Services.Analytics.Domain.Entities;

public class DashboardWidget
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid DashboardId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string WidgetType { get; set; } = string.Empty;
    public string? MetricName { get; set; }
    public string? Config { get; set; }
    public int Order { get; set; }
    
    // Navigation
    public Dashboard? Dashboard { get; set; }
}
