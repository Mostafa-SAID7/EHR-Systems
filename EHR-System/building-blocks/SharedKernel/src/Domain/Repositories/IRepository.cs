using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EHRPlatform.SharedKernel.Domain.Repositories;

/// <summary>
/// Generic repository interface for CRUD operations on aggregate roots.
/// </summary>
public interface IRepository<T> where T : BaseEntity
{
    /// <summary>
    /// Get entity by ID.
    /// </summary>
    Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all entities (including soft-deleted).
    /// </summary>
    Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get entities matching specification.
    /// </summary>
    Task<IReadOnlyList<T>> GetAsync(Specifications.Specification<T> spec, CancellationToken cancellationToken = default);

    /// <summary>
    /// Add entity to repository.
    /// </summary>
    Task AddAsync(T entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update existing entity.
    /// </summary>
    Task UpdateAsync(T entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete entity (hard delete).
    /// </summary>
    Task DeleteAsync(T entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Save all pending changes.
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
