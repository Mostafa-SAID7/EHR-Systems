#nullable enable

using EHRPlatform.Common.Entities;

namespace EHRPlatform.Common.Data;

/// <summary>
/// Generic repository interface for data access operations.
/// Supports CRUD operations, filtering, pagination, and soft delete.
/// HIPAA compliant with audit trail support via interceptors.
/// </summary>
/// <typeparam name="TEntity">Entity type that derives from BaseEntity</typeparam>
public interface IRepository<TEntity> where TEntity : BaseEntity
{
    /// <summary>
    /// Get entity by ID asynchronously.
    /// Returns null if entity not found or is soft-deleted.
    /// </summary>
    Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all entities (excluding soft-deleted).
    /// Use with caution on large datasets - prefer GetPagedAsync.
    /// </summary>
    Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get paginated list of entities with total count.
    /// Automatically excludes soft-deleted entities via global query filter.
    /// </summary>
    /// <param name="pageNumber">1-based page number</param>
    /// <param name="pageSize">Number of items per page (max 100)</param>
    /// <returns>Tuple of items and total count</returns>
    Task<(IEnumerable<TEntity> items, int totalCount)> GetPagedAsync(
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get entity by predicate.
    /// </summary>
    Task<TEntity?> FirstOrDefaultAsync(
        Func<IQueryable<TEntity>, IQueryable<TEntity>> predicate,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Add new entity to repository.
    /// Entity is not persisted until SaveChanges is called via Unit of Work.
    /// Sets CreatedAt and UpdatedAt to current UTC time.
    /// </summary>
    Task AddAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Add multiple entities in batch.
    /// </summary>
    Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update existing entity.
    /// Entity must already be tracked by DbContext.
    /// Sets UpdatedAt to current UTC time.
    /// Creates domain events for audit trail if applicable.
    /// </summary>
    Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Soft delete entity (sets DeletedAt, does not remove from database).
    /// Required for HIPAA audit trail - entity can be recovered if needed.
    /// </summary>
    Task DeleteAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Hard delete entity (physically removed from database).
    /// Use only for temporary data or during cleanup.
    /// HIPAA compliance: ensure audit log is preserved before hard delete.
    /// </summary>
    Task HardDeleteAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete multiple entities in batch (soft delete).
    /// </summary>
    Task DeleteRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get IQueryable for advanced LINQ queries.
    /// Use for complex filtering, projections, or includes.
    /// NOTE: Soft-deleted entities are automatically excluded via global query filter.
    /// </summary>
    /// <returns>IQueryable that excludes soft-deleted entities</returns>
    IQueryable<TEntity> AsQueryable();

    /// <summary>
    /// Check if entity exists by predicate.
    /// </summary>
    Task<bool> AnyAsync(
        Func<IQueryable<TEntity>, IQueryable<TEntity>> predicate,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get count of entities matching predicate.
    /// </summary>
    Task<int> CountAsync(
        Func<IQueryable<TEntity>, IQueryable<TEntity>>? predicate = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Restore soft-deleted entity (clears DeletedAt).
    /// Requires authorization - typically only admins or auditors.
    /// Creates audit log entry for recovery action.
    /// </summary>
    Task RestoreAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get deleted entities (where DeletedAt is not null).
    /// Used for audit recovery operations.
    /// </summary>
    Task<IEnumerable<TEntity>> GetDeletedAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get a filtered list of entities as a materialized List.
    /// Applies the query transform to AsQueryable() then executes ToListAsync.
    /// </summary>
    Task<List<TEntity>> ToListAsync(
        Func<IQueryable<TEntity>, IQueryable<TEntity>> query,
        CancellationToken cancellationToken = default);
}
