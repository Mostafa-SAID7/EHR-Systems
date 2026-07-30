using System.Linq.Expressions;

namespace EHRPlatform.Common.Specifications;

/// <summary>
/// Base specification pattern implementation.
/// Encapsulates query criteria, ordering, includes, and pagination for domain queries.
/// </summary>
/// <typeparam name="T">The entity type this specification applies to.</typeparam>
public abstract class Specification<T>
{
    private readonly List<Expression<Func<T, bool>>> _criteria = new();
    private readonly List<Expression<Func<T, object>>> _includes = new();
    private Expression<Func<T, object>>? _orderBy;
    private Expression<Func<T, object>>? _orderByDescending;
    private int? _take;
    private int? _skip;
    private bool _isPagingEnabled;

    /// <summary>Filter expressions that will be combined with AND logic.</summary>
    public IReadOnlyList<Expression<Func<T, bool>>> Criteria => _criteria.AsReadOnly();

    /// <summary>Navigation properties to eagerly load.</summary>
    public IReadOnlyList<Expression<Func<T, object>>> Includes => _includes.AsReadOnly();

    /// <summary>Ascending order expression, if set.</summary>
    public Expression<Func<T, object>>? OrderByExpression => _orderBy;

    /// <summary>Descending order expression, if set.</summary>
    public Expression<Func<T, object>>? OrderByDescendingExpression => _orderByDescending;

    /// <summary>Number of rows to take (for paging).</summary>
    public int? Take => _take;

    /// <summary>Number of rows to skip (for paging).</summary>
    public int? Skip => _skip;

    /// <summary>Whether paging is enabled on this specification.</summary>
    public bool IsPagingEnabled => _isPagingEnabled;

    /// <summary>Add a filter criterion. Multiple criteria are combined with AND.</summary>
    protected void AddCriteria(Expression<Func<T, bool>> criteria)
        => _criteria.Add(criteria);

    /// <summary>Add an eager-load include.</summary>
    protected void AddInclude(Expression<Func<T, object>> include)
        => _includes.Add(include);

    /// <summary>Set ascending order.</summary>
    protected void AddOrderBy(Expression<Func<T, object>> orderBy)
        => _orderBy = orderBy;

    /// <summary>Set descending order.</summary>
    protected void AddOrderByDescending(Expression<Func<T, object>> orderByDescending)
        => _orderByDescending = orderByDescending;

    /// <summary>Apply paging.</summary>
    protected void ApplyPaging(int skip, int take)
    {
        _skip = skip;
        _take = take;
        _isPagingEnabled = true;
    }

    /// <summary>Evaluate this specification against an in-memory entity (useful for unit tests).</summary>
    public bool IsSatisfiedBy(T entity)
        => _criteria.All(c => c.Compile()(entity));
}
