namespace EHRPlatform.Gateway.DTOs.Responses;

/// <summary>
/// Detailed gateway health response with full breakdown and recommendations.
/// </summary>
public class DetailedGatewayHealthResponse
{
    public string Status { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public Dictionary<string, ServiceHealthDetail> Services { get; set; } = new();
    public HealthSummary Summary { get; set; } = new();
    public bool IsCritical { get; set; }
}
