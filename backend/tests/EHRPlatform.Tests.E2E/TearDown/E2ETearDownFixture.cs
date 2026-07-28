using System;
using System.Threading.Tasks;

namespace EHRPlatform.Tests.E2E.TearDown;

/// <summary>
/// Tear down fixture for E2E tests - cleans up test environment
/// </summary>
public abstract class E2ETearDownFixture : IAsyncLifetime
{
    public virtual Task InitializeAsync()
    {
        return Task.CompletedTask;
    }

    public virtual async Task DisposeAsync()
    {
        await CleanupTestData();
        await CleanupTemporaryResources();
    }

    protected virtual async Task CleanupTestData()
    {
        // Override to clean up test data from databases
        await Task.CompletedTask;
    }

    protected virtual async Task CleanupTemporaryResources()
    {
        // Override to clean up temporary files, caches, etc.
        await Task.CompletedTask;
    }

    protected async Task<bool> WaitForResourceCleanup(Func<Task<bool>> cleanupCheck, int timeoutSeconds = 30)
    {
        var elapsed = 0;
        while (elapsed < timeoutSeconds)
        {
            if (await cleanupCheck())
            {
                return true;
            }
            await Task.Delay(1000);
            elapsed += 1;
        }
        return false;
    }
}
