namespace EHRPlatform.Gateway.DTOs.Responses;

/// <summary>
/// Health metrics suitable for monitoring dashboards.
/// </summary>
public class HealthMetrics
{
    public DateTime Timestamp { get; set; }
    public string GatewayStatus { get; set; } = string.Empty;
    public int TotalServices { get; set; }
    public int HealthyServices { get; set; }
    public int DegradedServices { get; set; }
    public int UnhealthyServices { get; set; }
    public double AverageResponseTime { get; set; }
    public double MaxResponseTime { get; set; }
    public double MinResponseTime { get; set; }
    public double HealthPercentage { get; set; }
}
