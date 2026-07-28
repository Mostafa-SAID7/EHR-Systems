using System;

namespace EHRPlatform.Tests.Contract.Fixtures;

/// <summary>
/// Base fixture for contract testing setup
/// </summary>
public abstract class ContractFixture : IDisposable
{
    protected bool Disposed { get; private set; }

    public virtual void Dispose()
    {
        if (!Disposed)
        {
            Disposed = true;
            GC.SuppressFinalize(this);
        }
    }
}
