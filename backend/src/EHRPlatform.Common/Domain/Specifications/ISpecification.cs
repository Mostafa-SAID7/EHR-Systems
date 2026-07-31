#nullable enable

using System.Linq.Expressions;

namespace EHRPlatform.Common.Domain.Specifications;

/// <summary>
/// Contract for the Specification pattern.
/// Encapsulates query criteria, ordering, includes, and pagination for domain queries.
/// Single responsibility: Define specification query contract only.
/// </summary>
/// <typeparam name="T">The entity type this specification applies to.</typeparam>
public interface ISpecification<T>
{
    /// <summary>
    /// Filter expressions that will be combined with AND logic.
    /// </summary>
    IReadOnlyList<Expression<Func<T, bool>>> Criteria { get; }

    /// <summary>
    /// Navigation properties to eagerly load.
    /// </summary>
    IReadOnlyList<Expression<Func<T, object>>> Includes { get; }

    /// <summary>
    /// Ascending order expression, if set.
    /// </summary>
    Expression<Func<T, object>>? OrderByExpression { get; }

    /// <summary>
    /// Descending order expression, if set.
    /// </summary>
    Expression<Func<T, object>>? OrderByDescendingExpression { get; }

    /// <summary>
    /// Number of rows to take (for paging).
    /// </summary>
    int? Take { get; }

    /// <summary>
    /// Number of rows to skip (for paging).
    /// </summary>
    int? Skip { get; }

    /// <summary>
    /// Whether paging is enabled on this specification.
    /// </summary>
    bool IsPagingEnabled { get; }

    /// <summary>
    /// Evaluate this specification against an in-memory entity (useful for unit tests).
    /// </summary>
    bool IsSatisfiedBy(T entity);
}
