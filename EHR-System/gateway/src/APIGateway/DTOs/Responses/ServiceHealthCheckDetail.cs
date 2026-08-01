namespace EHRPlatform.Gateway.DTOs.Responses;

/// <summary>
/// Service health check detail with basic information.
/// </summary>
public class ServiceHealthCheckDetail
{
    public string Name { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public double Duration { get; set; }
    public Dictionary<string, string> Data { get; set; } = new();
}
