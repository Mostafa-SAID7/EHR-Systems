namespace EHRPlatform.Gateway.Infrastructure.HealthChecks.Models;

/// <summary>
/// Gateway health response model.
/// </summary>
public class GatewayHealthResponse
{
    public string OverallStatus { get; set; } = "Unknown";
    public DateTime Timestamp { get; set; }
    public Dictionary<string, ServiceHealthStatus> Services { get; set; } = new();
    public int TotalServices { get; set; }
    public int HealthyServices { get; set; }
    public int DegradedServices { get; set; }
    public int UnhealthyServices { get; set; }
}
