using System;
using System.Net.Http;
using Xunit;

namespace EHRPlatform.Tests.E2E;

/// <summary>
/// Base class for end-to-end tests
/// </summary>
public abstract class E2ETestBase : IAsyncLifetime
{
    protected HttpClient HttpClient { get; set; }
    protected string BaseUrl { get; set; }

    public virtual Task InitializeAsync()
    {
        HttpClient = new HttpClient();
        return Task.CompletedTask;
    }

    public virtual Task DisposeAsync()
    {
        HttpClient?.Dispose();
        return Task.CompletedTask;
    }

    protected async Task<T> GetAsync<T>(string endpoint)
    {
        var response = await HttpClient.GetAsync($"{BaseUrl}/{endpoint}");
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        return System.Text.Json.JsonSerializer.Deserialize<T>(content);
    }

    protected async Task<T> PostAsync<T>(string endpoint, object data)
    {
        var json = System.Text.Json.JsonSerializer.Serialize(data);
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
        var response = await HttpClient.PostAsync($"{BaseUrl}/{endpoint}", content);
        response.EnsureSuccessStatusCode();
        var responseContent = await response.Content.ReadAsStringAsync();
        return System.Text.Json.JsonSerializer.Deserialize<T>(responseContent);
    }
}
