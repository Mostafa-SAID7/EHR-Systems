namespace EHRPlatform.Gateway.DTOs.Responses;

/// <summary>
/// Gateway health check response with service details.
/// </summary>
public class GatewayHealthCheckResponse
{
    public string Status { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public List<ServiceHealthCheckDetail> Services { get; set; } = new();
    public HealthSummary Summary { get; set; } = new();
}
