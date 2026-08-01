using EHRPlatform.Gateway.Models;

namespace EHRPlatform.Gateway.DTOs.Responses;

/// <summary>
/// Analytics dashboard response - system KPIs aggregation.
/// </summary>
public class AnalyticsDashboardResponse
{
    public SystemKpis? Kpis { get; set; }
    public DateTime GeneratedAt { get; set; }
    public string TraceId { get; set; } = string.Empty;
}
