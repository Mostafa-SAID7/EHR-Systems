using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace EHRPlatform.Tests.Common.Fixtures;

/// <summary>
/// Fixture for HTTP client testing
/// </summary>
public abstract class HttpClientFixture : IAsyncLifetime
{
    protected HttpClient HttpClient { get; set; }
    protected HttpClientHandler HttpClientHandler { get; set; }

    public virtual Task InitializeAsync()
    {
        HttpClientHandler = new HttpClientHandler();
        HttpClient = new HttpClient(HttpClientHandler);
        HttpClient.Timeout = TimeSpan.FromSeconds(30);
        return Task.CompletedTask;
    }

    public virtual Task DisposeAsync()
    {
        HttpClient?.Dispose();
        HttpClientHandler?.Dispose();
        return Task.CompletedTask;
    }
}
