using System;
using System.Threading.Tasks;

namespace EHRPlatform.Tests.E2E.SetUp;

/// <summary>
/// Setup fixture for E2E tests - initializes test environment
/// </summary>
public abstract class E2ESetupFixture : IAsyncLifetime
{
    public string BaseUrl { get; protected set; }
    public string AdminToken { get; protected set; }
    public string UserToken { get; protected set; }

    public virtual Task InitializeAsync()
    {
        BaseUrl = Environment.GetEnvironmentVariable("TEST_BASE_URL") ?? "http://localhost:5000";
        return Task.CompletedTask;
    }

    public virtual Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    protected virtual async Task AuthenticateAsAdmin()
    {
        AdminToken = await GetAuthToken("admin@test.com", "AdminPassword123!");
    }

    protected virtual async Task AuthenticateAsUser(string email, string password)
    {
        UserToken = await GetAuthToken(email, password);
    }

    protected virtual async Task<string> GetAuthToken(string email, string password)
    {
        // This would be implemented to call the actual authentication endpoint
        return await Task.FromResult($"token_for_{email}");
    }
}
