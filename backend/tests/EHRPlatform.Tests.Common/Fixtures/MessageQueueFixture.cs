using System;
using System.Threading.Tasks;

namespace EHRPlatform.Tests.Common.Fixtures;

/// <summary>
/// Base fixture for message queue testing
/// </summary>
public abstract class MessageQueueFixture : IAsyncLifetime
{
    public virtual Task InitializeAsync()
    {
        return Task.CompletedTask;
    }

    public virtual Task DisposeAsync()
    {
        return Task.CompletedTask;
    }
}
