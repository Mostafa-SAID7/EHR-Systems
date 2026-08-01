using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace EHRPlatform.SharedKernel.Domain.Specifications;

/// <summary>
/// Base Specification pattern for DDD.
/// Encapsulates business logic for filtering, sorting, and including related data.
/// 
/// Example usage:
/// <code>
/// var spec = new GetActivePatientsByNameSpecification("John");
/// var patients = await _repository.GetAsync(spec);
/// </code>
/// 
/// This separates query logic from repositories, making code more testable and reusable.
/// </summary>
public abstract class Specification<T> where T : BaseEntity
{
    /// <summary>
    /// The main filter criteria.
    /// </summary>
    public Expression<Func<T, bool>>? Criteria { get; protected set; }

    /// <summary>
    /// Included related entities (eager loading).
    /// </summary>
    public List<Expression<Func<T, object>>> Includes { get; } = new();

    /// <summary>
    /// String-based includes for complex navigation paths.
    /// </summary>
    public List<string> IncludeStrings { get; } = new();

    /// <summary>
    /// Order by expression.
    /// </summary>
    public Expression<Func<T, object>>? OrderBy { get; protected set; }

    /// <summary>
    /// Order by descending expression.
    /// </summary>
    public Expression<Func<T, object>>? OrderByDescending { get; protected set; }

    /// <summary>
    /// Pagination: number of records to skip.
    /// </summary>
    public int? Skip { get; protected set; }

    /// <summary>
    /// Pagination: number of records to take.
    /// </summary>
    public int? Take { get; protected set; }

    /// <summary>
    /// Whether to use pagination.
    /// </summary>
    public bool IsPagingEnabled { get; protected set; }

    /// <summary>
    /// Add an include for related entity.
    /// </summary>
    protected virtual void AddInclude(Expression<Func<T, object>> includeExpression)
    {
        Includes.Add(includeExpression);
    }

    /// <summary>
    /// Add a string-based include (for complex navigation paths).
    /// </summary>
    protected virtual void AddInclude(string includeString)
    {
        IncludeStrings.Add(includeString);
    }

    /// <summary>
    /// Apply pagination.
    /// </summary>
    protected virtual void ApplyPaging(int skip, int take)
    {
        Skip = skip;
        Take = take;
        IsPagingEnabled = true;
    }
}

/// <summary>
/// Specification with select projection (for DTOs).
/// </summary>
public abstract class Specification<T, TResult> : Specification<T> where T : BaseEntity
{
    /// <summary>
    /// Select projection to DTO or custom result type.
    /// </summary>
    public Expression<Func<T, TResult>>? Select { get; protected set; }
}
