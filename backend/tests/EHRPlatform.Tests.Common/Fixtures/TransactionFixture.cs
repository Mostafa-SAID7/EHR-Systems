using System;
using System.Threading.Tasks;

namespace EHRPlatform.Tests.Common.Fixtures;

/// <summary>
/// Fixture for transactional database testing with rollback support
/// </summary>
public abstract class TransactionFixture : IAsyncLifetime
{
    protected string TransactionId { get; set; }
    protected DateTime StartTime { get; set; }
    protected bool RollbackEnabled { get; set; } = true;

    public virtual Task InitializeAsync()
    {
        TransactionId = Guid.NewGuid().ToString();
        StartTime = DateTime.UtcNow;
        return Task.CompletedTask;
    }

    public virtual Task DisposeAsync()
    {
        if (RollbackEnabled)
        {
            return RollbackChangesAsync();
        }
        return Task.CompletedTask;
    }

    protected virtual async Task RollbackChangesAsync()
    {
        await Task.CompletedTask;
    }

    protected virtual void DisableRollback()
    {
        RollbackEnabled = false;
    }

    protected virtual void EnableRollback()
    {
        RollbackEnabled = true;
    }
}
