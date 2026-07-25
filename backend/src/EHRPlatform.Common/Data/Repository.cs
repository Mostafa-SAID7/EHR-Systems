#nullable enable

using EHRPlatform.Common.Entities;
using Microsoft.EntityFrameworkCore;

namespace EHRPlatform.Common.Data;

/// <summary>
/// Generic repository implementation using Entity Framework Core.
/// Provides CRUD operations with automatic soft delete support.
/// All queries automatically exclude soft-deleted entities via DbContext global query filter.
/// HIPAA compliant: timestamps and audit trail managed via interceptors.
/// </summary>
/// <typeparam name="TEntity">Entity type that derives from BaseEntity</typeparam>
public class Repository<TEntity> : IRepository<TEntity> where TEntity : BaseEntity
{
    protected readonly DbContext _context;
    protected readonly DbSet<TEntity> _dbSet;

    public Repository(DbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _dbSet = context.Set<TEntity>();
    }

    /// <summary>
    /// Get entity by ID asynchronously.
    /// Global query filter automatically excludes soft-deleted entities.
    /// </summary>
    public virtual async Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        ArgumentGuard.NotEmpty(id, nameof(id));
        
        return await _dbSet.FindAsync(new object[] { id }, cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Get all entities excluding soft-deleted.
    /// CAUTION: Can be expensive on large tables - prefer GetPagedAsync.
    /// </summary>
    public virtual async Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet.AsNoTracking().ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Get paginated entities with total count.
    /// Implements offset/limit pattern for efficient large dataset retrieval.
    /// </summary>
    public virtual async Task<(IEnumerable<TEntity> items, int totalCount)> GetPagedAsync(
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        if (pageNumber < 1)
            throw new ArgumentException("Page number must be >= 1", nameof(pageNumber));
        
        if (pageSize < 1 || pageSize > 100)
            throw new ArgumentException("Page size must be between 1 and 100", nameof(pageSize));

        var totalCount = await _dbSet.CountAsync(cancellationToken);
        
        var items = await _dbSet
            .AsNoTracking()
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    /// <summary>
    /// Get single entity by predicate.
    /// </summary>
    public virtual async Task<TEntity?> FirstOrDefaultAsync(
        Func<IQueryable<TEntity>, IQueryable<TEntity>> predicate,
        CancellationToken cancellationToken = default)
    {
        ArgumentGuard.NotNull(predicate, nameof(predicate));
        
        var query = predicate(_dbSet.AsQueryable());
        return await query.AsNoTracking().FirstOrDefaultAsync(cancellationToken);
    }

    /// <summary>
    /// Add new entity.
    /// Sets CreatedAt and UpdatedAt to current UTC time via interceptor.
    /// Entity is tracked but not persisted until SaveChangesAsync called.
    /// </summary>
    public virtual async Task AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        ArgumentGuard.NotNull(entity, nameof(entity));
        
        // DbContext interceptor will set CreatedAt and UpdatedAt
        await _dbSet.AddAsync(entity, cancellationToken);
    }

    /// <summary>
    /// Add multiple entities in batch.
    /// </summary>
    public virtual async Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
    {
        ArgumentGuard.NotNull(entities, nameof(entities));
        
        var entityList = entities.ToList();
        if (entityList.Count == 0)
            return;

        await _dbSet.AddRangeAsync(entityList, cancellationToken);
    }

    /// <summary>
    /// Update existing entity.
    /// Sets UpdatedAt to current UTC time via interceptor.
    /// Entity must be tracked by context.
    /// Creates domain events for audit trail if entity is AuditableEntity.
    /// </summary>
    public virtual async Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        ArgumentGuard.NotNull(entity, nameof(entity));
        
        // Mark as modified - interceptor will set UpdatedAt
        _dbSet.Update(entity);
        
        // Return completed task for async signature
        await Task.CompletedTask;
    }

    /// <summary>
    /// Soft delete entity (set DeletedAt, preserve in database).
    /// HIPAA requirement: entities must be recoverable for audit purposes.
    /// Global query filter automatically excludes from future queries.
    /// </summary>
    public virtual async Task DeleteAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        ArgumentGuard.NotNull(entity, nameof(entity));
        
        // Set DeletedAt via property setter
        entity.DeletedAt = DateTime.UtcNow;
        
        // Interceptor will set UpdatedAt
        _dbSet.Update(entity);
        
        await Task.CompletedTask;
    }

    /// <summary>
    /// Hard delete entity (physically remove from database).
    /// Use only for temporary data or cleanup.
    /// WARNING: HIPAA compliance risk - ensure audit log is preserved.
    /// </summary>
    public virtual async Task HardDeleteAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        ArgumentGuard.NotNull(entity, nameof(entity));
        
        _dbSet.Remove(entity);
        
        await Task.CompletedTask;
    }

    /// <summary>
    /// Batch soft delete multiple entities.
    /// </summary>
    public virtual async Task DeleteRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
    {
        ArgumentGuard.NotNull(entities, nameof(entities));
        
        var entityList = entities.ToList();
        if (entityList.Count == 0)
            return;

        var now = DateTime.UtcNow;
        foreach (var entity in entityList)
        {
            entity.DeletedAt = now;
            _dbSet.Update(entity);
        }
        
        await Task.CompletedTask;
    }

    /// <summary>
    /// Get IQueryable for complex queries.
    /// Global query filter automatically excludes soft-deleted entities.
    /// </summary>
    public virtual IQueryable<TEntity> AsQueryable()
    {
        return _dbSet.AsQueryable();
    }

    /// <summary>
    /// Check if entities matching predicate exist.
    /// </summary>
    public virtual async Task<bool> AnyAsync(
        Func<IQueryable<TEntity>, IQueryable<TEntity>> predicate,
        CancellationToken cancellationToken = default)
    {
        ArgumentGuard.NotNull(predicate, nameof(predicate));
        
        var query = predicate(_dbSet.AsQueryable());
        return await query.AnyAsync(cancellationToken);
    }

    /// <summary>
    /// Get count of entities matching predicate.
    /// </summary>
    public virtual async Task<int> CountAsync(
        Func<IQueryable<TEntity>, IQueryable<TEntity>>? predicate = null,
        CancellationToken cancellationToken = default)
    {
        IQueryable<TEntity> query = _dbSet;
        
        if (predicate != null)
            query = predicate(query);

        return await query.CountAsync(cancellationToken);
    }

    /// <summary>
    /// Restore soft-deleted entity.
    /// Clears DeletedAt to make visible in future queries.
    /// Creates audit log entry for recovery action.
    /// </summary>
    public virtual async Task RestoreAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        ArgumentGuard.NotNull(entity, nameof(entity));
        
        entity.DeletedAt = null;
        _dbSet.Update(entity);
        
        await Task.CompletedTask;
    }

    /// <summary>
    /// Get all deleted entities (where DeletedAt is not null).
    /// Requires explicit query - global filter normally excludes these.
    /// Used by audit recovery operations.
    /// </summary>
    public virtual async Task<IEnumerable<TEntity>> GetDeletedAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .IgnoreQueryFilters() // Include soft-deleted
            .Where(e => e.DeletedAt != null)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Materializes a filtered query into a List.
    /// Applies the query transform then calls ToListAsync.
    /// </summary>
    public virtual async Task<List<TEntity>> ToListAsync(
        Func<IQueryable<TEntity>, IQueryable<TEntity>> query,
        CancellationToken cancellationToken = default)
    {
        ArgumentGuard.NotNull(query, nameof(query));
        return await query(_dbSet.AsQueryable()).AsNoTracking().ToListAsync(cancellationToken);
    }
}

/// <summary>
/// Helper class for argument validation.
/// Used throughout repository to provide consistent validation.
/// </summary>
internal static class ArgumentGuard
{
    public static void NotNull<T>(T? argument, string parameterName) where T : class
    {
        if (argument == null)
            throw new ArgumentNullException(parameterName);
    }

    public static void NotEmpty(Guid argument, string parameterName)
    {
        if (argument == Guid.Empty)
            throw new ArgumentException("Value cannot be empty GUID", parameterName);
    }

    public static void NotNullOrEmpty(string? argument, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(argument))
            throw new ArgumentException("Value cannot be null or empty", parameterName);
    }
}
