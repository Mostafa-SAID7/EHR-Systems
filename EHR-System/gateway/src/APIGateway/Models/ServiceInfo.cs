namespace EHRPlatform.Gateway.Models;

/// <summary>
/// Service registry information.
/// </summary>
public class ServiceInfo
{
    public string Name { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = string.Empty;
    public int Port { get; set; }
    public bool IsHealthy { get; set; }
}
