namespace EHRPlatform.Gateway.Models;

/// <summary>
/// Service configuration DTO for health check endpoint.
/// </summary>
public class ServiceConfigDto
{
    public string Name { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = string.Empty;
    public int Port { get; set; }
    public string Version { get; set; } = "1.0";
    public bool Critical { get; set; } = true;
}
