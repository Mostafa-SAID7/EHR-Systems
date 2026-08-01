namespace EHRPlatform.Gateway.DTOs.Responses;

/// <summary>
/// Summary of health check statistics.
/// </summary>
public class HealthSummary
{
    public int Total { get; set; }
    public int Healthy { get; set; }
    public int Degraded { get; set; }
    public int Unhealthy { get; set; }
}
