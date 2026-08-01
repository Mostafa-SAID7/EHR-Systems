using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace EHRPlatform.SharedKernel.Specifications;

/// <summary>
/// Interface for specification pattern.
/// Single responsibility: Query specification contract.
/// </summary>
public interface ISpecification<T> where T : class
{
    /// <summary>
    /// Criteria expression.
    /// </summary>
    Expression<Func<T, bool>> Criteria { get; }

    /// <summary>
    /// Include expressions for navigation properties.
    /// </summary>
    List<Expression<Func<T, object>>> Includes { get; }

    /// <summary>
    /// Include expressions (string paths).
    /// </summary>
    List<string> IncludeStrings { get; }

    /// <summary>
    /// Order by expressions.
    /// </summary>
    List<(Expression<Func<T, object>> KeySelector, bool Descending)> OrderBy { get; }

    /// <summary>
    /// Pagination: number of records to take.
    /// </summary>
    int? Take { get; }

    /// <summary>
    /// Pagination: number of records to skip.
    /// </summary>
    int? Skip { get; }

    /// <summary>
    /// Whether pagination is enabled.
    /// </summary>
    bool IsPagingEnabled { get; }

    /// <summary>
    /// Whether to track entities.
    /// </summary>
    bool AsNoTracking { get; }
}
