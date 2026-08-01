namespace EHRPlatform.Gateway.DTOs.Responses;

/// <summary>
/// Service-specific health response.
/// </summary>
public class ServiceHealthResponse
{
    public string ServiceName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public double Duration { get; set; }
    public Dictionary<string, string> Data { get; set; } = new();
    public DateTime Timestamp { get; set; }
}
