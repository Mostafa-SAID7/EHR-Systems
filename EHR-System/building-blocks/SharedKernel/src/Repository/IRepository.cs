using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using EHRPlatform.SharedKernel.Domain;

namespace EHRPlatform.SharedKernel.Repository;

/// <summary>
/// Generic repository interface for data access abstraction.
/// Single responsibility: Repository contract for aggregate roots.
/// </summary>
public interface IRepository<T> where T : AggregateRoot
{
    /// <summary>
    /// Get entity by ID.
    /// </summary>
    Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all entities.
    /// </summary>
    Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Find entities matching specification.
    /// </summary>
    Task<IReadOnlyList<T>> GetAsync(ISpecification<T> specification, CancellationToken cancellationToken = default);

    /// <summary>
    /// Find single entity matching specification.
    /// </summary>
    Task<T?> GetSingleAsync(ISpecification<T> specification, CancellationToken cancellationToken = default);

    /// <summary>
    /// Find entities by predicate.
    /// </summary>
    Task<IReadOnlyList<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Count total entities.
    /// </summary>
    Task<int> CountAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Count entities matching specification.
    /// </summary>
    Task<int> CountAsync(ISpecification<T> specification, CancellationToken cancellationToken = default);

    /// <summary>
    /// Add entity to repository.
    /// </summary>
    Task<T> AddAsync(T entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Add multiple entities.
    /// </summary>
    Task AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update existing entity.
    /// </summary>
    Task<T> UpdateAsync(T entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete entity by ID.
    /// </summary>
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete entity.
    /// </summary>
    Task<bool> DeleteAsync(T entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete multiple entities.
    /// </summary>
    Task<int> DeleteRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if entity exists.
    /// </summary>
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if any entity matches predicate.
    /// </summary>
    Task<bool> AnyAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
}

/// <summary>
/// Base class for aggregate root entities.
/// </summary>
public abstract class AggregateRoot : AuditableEntity
{
    protected AggregateRoot()
    {
    }

    protected AggregateRoot(Guid id) : base(id)
    {
    }
}
