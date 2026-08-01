using System;
using System.Threading;
using System.Threading.Tasks;

namespace EHRPlatform.SharedKernel.UnitOfWork;

/// <summary>
/// Unit of Work pattern for managing transactions across repositories.
/// Single responsibility: Transaction coordination contract.
/// </summary>
public interface IUnitOfWork : IDisposable
{
    /// <summary>
    /// Begin a transaction.
    /// </summary>
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Commit all changes to database.
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Commit transaction.
    /// </summary>
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Rollback transaction.
    /// </summary>
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if transaction is active.
    /// </summary>
    bool HasActiveTransaction { get; }

    /// <summary>
    /// Execute work within a transaction context.
    /// </summary>
    Task<T> ExecuteAsync<T>(Func<Task<T>> work, CancellationToken cancellationToken = default);

    /// <summary>
    /// Execute work without return value within transaction.
    /// </summary>
    Task ExecuteAsync(Func<Task> work, CancellationToken cancellationToken = default);
}
