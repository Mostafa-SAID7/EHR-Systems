namespace Identity.Domain.Interfaces;

using Identity.Domain.Entities;

/// <summary>
/// Unit of Work pattern interface for managing repository transactions
/// </summary>
public interface IUnitOfWork : IAsyncDisposable
{
    /// <summary>
    /// Gets the user repository
    /// </summary>
    IUserRepository Users { get; }

    /// <summary>
    /// Gets the role repository
    /// </summary>
    IRepository<Role, Guid> Roles { get; }

    /// <summary>
    /// Saves all changes to the database
    /// </summary>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>The number of entities changed</returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Begins a transaction
    /// </summary>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>A transaction handle</returns>
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Commits the current transaction
    /// </summary>
    /// <param name="cancellationToken">The cancellation token</param>
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Rolls back the current transaction
    /// </summary>
    /// <param name="cancellationToken">The cancellation token</param>
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}
