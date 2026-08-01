using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EHRPlatform.SharedKernel.Domain.Repositories;

/// <summary>
/// Specification repository for DDD specification pattern queries.
/// Separates complex query logic from repositories.
/// </summary>
public interface ISpecificationRepository<T> where T : BaseEntity
{
    /// <summary>
    /// Get single entity matching specification.
    /// </summary>
    Task<T?> GetAsync(Specifications.Specification<T> spec, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all entities matching specification.
    /// </summary>
    Task<IReadOnlyList<T>> ListAsync(Specifications.Specification<T> spec, CancellationToken cancellationToken = default);

    /// <summary>
    /// Count entities matching specification.
    /// </summary>
    Task<int> CountAsync(Specifications.Specification<T> spec, CancellationToken cancellationToken = default);
}
