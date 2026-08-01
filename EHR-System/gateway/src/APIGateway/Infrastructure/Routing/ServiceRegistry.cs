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

public class ServiceRegistry : IServiceRegistry
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<ServiceRegistry> _logger;
    private readonly Dictionary<string, string> _serviceUrls;

    public ServiceRegistry(IConfiguration configuration, ILogger<ServiceRegistry> logger)
    {
        _configuration = configuration;
        _logger = logger;
        _serviceUrls = new Dictionary<string, string>();

        InitializeServices();
    }

    private void InitializeServices()
    {
        // Load from configuration
        var services = _configuration.GetSection("Services").Get<Dictionary<string, string>>() 
                    ?? new Dictionary<string, string>();

        foreach (var service in services)
        {
            _serviceUrls[service.Key] = service.Value;
            _logger.LogInformation("Registered service: {Service} → {Url}", service.Key, service.Value);
        }

        // Defaults (if not in config)
        SetDefault("identity", "http://localhost:5003");
        SetDefault("patient", "http://localhost:5004");
        SetDefault("audit", "http://localhost:5005");
        SetDefault("appointment", "http://localhost:5006");
        SetDefault("notification", "http://localhost:5007");
        SetDefault("analytics", "http://localhost:5008");
        SetDefault("clinical", "http://localhost:5001");
        SetDefault("billing", "http://localhost:5002");
        SetDefault("file-storage", "http://localhost:5009");
        SetDefault("terminology", "http://localhost:5010");
        SetDefault("integration", "http://localhost:5011");
        SetDefault("ai", "http://localhost:5012");
    }

    private void SetDefault(string name, string url)
    {
        if (!_serviceUrls.ContainsKey(name))
        {
            _serviceUrls[name] = url;
        }
    }

    public string GetServiceUrl(string serviceName)
    {
        if (_serviceUrls.TryGetValue(serviceName.ToLower(), out var url))
        {
            return url;
        }

        throw new KeyNotFoundException($"Service '{serviceName}' not found in registry");
    }

    public bool IsServiceAvailable(string serviceName)
    {
        return _serviceUrls.ContainsKey(serviceName.ToLower());
    }

    public void RegisterService(string name, string url)
    {
        _serviceUrls[name.ToLower()] = url;
        _logger.LogInformation("Dynamically registered service: {Service} → {Url}", name, url);
    }
}
