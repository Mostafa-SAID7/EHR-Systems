namespace EHRPlatform.Gateway.Infrastructure.HealthChecks.Models;

/// <summary>
/// Individual service health status.
/// </summary>
public class ServiceHealthStatus
{
    public string Name { get; set; } = string.Empty;
    public string Status { get; set; } = "Unknown";
    public string ResponseTime { get; set; } = "N/A";
    public string Description { get; set; } = string.Empty;
    public DateTime LastChecked { get; set; }
}
