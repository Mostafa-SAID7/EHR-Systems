namespace EHRPlatform.Gateway.DTOs.Responses;

/// <summary>
/// Detailed service health information including recommendations.
/// </summary>
public class ServiceHealthDetail : ServiceHealthCheckDetail
{
    public DateTime LastUpdated { get; set; }
    public string Recommendation { get; set; } = string.Empty;
}
