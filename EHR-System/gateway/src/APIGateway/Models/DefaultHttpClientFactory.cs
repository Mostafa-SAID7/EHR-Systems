namespace EHRPlatform.Gateway.Models;

/// <summary>
/// Default HTTP client factory for health check dependencies.
/// </summary>
public class DefaultHttpClientFactory : IHttpClientFactory
{
    public HttpClient CreateClient(string name)
    {
        return new HttpClient();
    }
}
