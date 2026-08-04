namespace EHRPlatform.Services.Analytics.Domain.Entities;

/// <summary>
/// DashboardWidget - Widget on a dashboard
/// </summary>
public class DashboardWidget
{
    public Guid Id { get; set; }
    public Guid DashboardId { get; set; }
    public string WidgetType { get; set; } = string.Empty; // KPI, LineChart, BarChart, Table, Gauge
    public string MetricName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public int PositionX { get; set; }
    public int PositionY { get; set; }
    public int Width { get; set; } = 4;
    public int Height { get; set; } = 2;
    public string? Configuration { get; set; } // JSON for chart config
    public DateTime CreatedAt { get; set; }

    public Dashboard Dashboard { get; set; } = null!;
}
