using System;
using System.Threading;
using System.Threading.Tasks;

namespace EHRPlatform.SharedKernel.Domain.Repositories;

/// <summary>
/// Unit of work for transaction coordination across multiple repositories.
/// </summary>
public interface IUnitOfWork : IDisposable, IAsyncDisposable
{
    /// <summary>
    /// Get repository for entity type.
    /// </summary>
    IRepository<T> Repository<T>() where T : BaseEntity;

    /// <summary>
    /// Save all pending changes in transaction.
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Begin database transaction.
    /// </summary>
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Commit transaction.
    /// </summary>
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Rollback transaction.
    /// </summary>
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}
