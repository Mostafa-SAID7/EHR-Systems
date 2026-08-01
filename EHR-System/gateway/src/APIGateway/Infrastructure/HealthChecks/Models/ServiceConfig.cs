namespace EHRPlatform.Gateway.Infrastructure.HealthChecks.Models;

/// <summary>
/// Service configuration for health check endpoint.
/// </summary>
public class ServiceConfig
{
    public string Name { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = string.Empty;
    public int Port { get; set; }
    public string Version { get; set; } = "1.0";
    public bool Critical { get; set; } = true; // If false, system can operate without this service
}
