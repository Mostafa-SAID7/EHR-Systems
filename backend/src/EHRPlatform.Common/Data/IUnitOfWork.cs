#nullable enable

using EHRPlatform.Common.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace EHRPlatform.Common.Data;

/// <summary>
/// Unit of Work pattern interface for managing database transactions and repositories.
/// Ensures ACID compliance and enables rollback on errors.
/// HIPAA compliant: all changes are audited via interceptors and tracked via transactions.
/// 
/// Usage:
/// using (var uow = new UnitOfWork(context))
/// {
///     var repo = uow.Repository<Patient>();
///     var patient = new Patient { ... };
///     await repo.AddAsync(patient);
///     await uow.SaveChangesAsync(ct);
/// }
/// </summary>
public interface IUnitOfWork : IDisposable, IAsyncDisposable
{
    /// <summary>
    /// Get underlying DbContext for direct access if needed.
    /// Use sparingly - prefer GetRepository pattern.
    /// </summary>
    DbContext DbContext { get; }

    /// <summary>
    /// Get or create repository for entity type.
    /// Repositories are cached within the UnitOfWork instance.
    /// Each entity type has its own repository instance.
    /// </summary>
    /// <typeparam name="TEntity">Entity type deriving from BaseEntity</typeparam>
    /// <returns>Repository instance for entity type</returns>
    IRepository<TEntity> Repository<TEntity>() where TEntity : BaseEntity;

    /// <summary>
    /// Begin a new database transaction.
    /// Use for operations that require rollback capability.
    /// IMPORTANT: Only one active transaction allowed at a time.
    /// </summary>
    /// <returns>Transaction token for tracking</returns>
    Task<IDbContextTransaction?> BeginTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Commit the current transaction.
    /// Applies all changes to database.
    /// If no transaction exists, this is a no-op.
    /// </summary>
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Rollback the current transaction.
    /// Reverts all changes made since transaction began.
    /// If no transaction exists, this is a no-op.
    /// </summary>
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Save all pending changes to database.
    /// Calls SaveChangesAsync on DbContext.
    /// HIPAA: All changes are audited via interceptors before commit.
    /// 
    /// Behavior:
    /// - If in transaction: changes committed to transaction
    /// - If no transaction: auto-commits to database
    /// - Throws DbUpdateException if constraint violations occur
    /// - Throws DbUpdateConcurrencyException if concurrency conflicts occur
    /// </summary>
    /// <returns>Number of entities changed</returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Save changes and publish domain events to outbox.
    /// Used in CQRS command handlers where domain events must be published.
    /// Events are stored in outbox table with same transaction.
    /// BackgroundService processes outbox and publishes to Kafka.
    /// </summary>
    /// <returns>Tuple of (entities changed, events published to outbox)</returns>
    Task<(int changesCount, int eventsCount)> SaveChangesWithEventPublishingAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if there are pending changes not yet saved.
    /// Useful for detecting unsaved modifications before commit.
    /// </summary>
    bool HasPendingChanges();

    /// <summary>
    /// Execute action within a transaction scope.
    /// Automatically commits on success, rolls back on error.
    /// Simpler than BeginTransaction/CommitTransaction pattern.
    /// 
    /// Example:
    /// await uow.ExecuteInTransactionAsync(async () =>
    /// {
    ///     await repo.AddAsync(entity);
    ///     await uow.SaveChangesAsync(ct);
    /// });
    /// </summary>
    Task ExecuteInTransactionAsync(
        Func<IUnitOfWork, Task> action,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Execute action within a transaction scope with return value.
    /// </summary>
    Task<T> ExecuteInTransactionAsync<T>(
        Func<IUnitOfWork, Task<T>> action,
        CancellationToken cancellationToken = default);
}

