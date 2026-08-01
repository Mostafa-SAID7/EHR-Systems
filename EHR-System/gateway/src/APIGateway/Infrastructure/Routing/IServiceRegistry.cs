namespace EHRPlatform.Gateway.Infrastructure.Routing;

/// <summary>
/// Service registry mapping routes to microservices.
/// Centralized configuration of all service endpoints.
/// </summary>
public interface IServiceRegistry
{
    string GetServiceUrl(string serviceName);
    bool IsServiceAvailable(string serviceName);
    void RegisterService(string name, string url);
}
