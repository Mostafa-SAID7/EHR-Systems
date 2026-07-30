#nullable enable

namespace EHRPlatform.Common.Data.Abstractions;

/// <summary>
/// Thin Dapper façade over the service's existing database connection.
/// Use for complex reporting queries, bulk operations, and anything where
/// EF Core's LINQ translation produces inefficient SQL.
///
/// The connection is owned by the EF Core DbContext so it participates in
/// the same transaction when one is open via <see cref="IUnitOfWork"/>.
/// </summary>
public interface IDapperContext
{
    /// <summary>Execute a query and return a typed sequence.</summary>
    Task<IEnumerable<T>> QueryAsync<T>(
        string sql,
        object? parameters          = null,
        CancellationToken ct        = default);

    /// <summary>Return the first row or default.</summary>
    Task<T?> QueryFirstOrDefaultAsync<T>(
        string sql,
        object? parameters          = null,
        CancellationToken ct        = default);

    /// <summary>Execute a non-query (INSERT/UPDATE/DELETE) and return rows affected.</summary>
    Task<int> ExecuteAsync(
        string sql,
        object? parameters          = null,
        CancellationToken ct        = default);

    /// <summary>Execute a scalar query (COUNT, SUM, …).</summary>
    Task<T?> ExecuteScalarAsync<T>(
        string sql,
        object? parameters          = null,
        CancellationToken ct        = default);

    /// <summary>
    /// Multi-result query — maps two joined tables into a single result type.
    /// </summary>
    Task<IEnumerable<TReturn>> QueryAsync<TFirst, TSecond, TReturn>(
        string sql,
        Func<TFirst, TSecond, TReturn> map,
        object? parameters          = null,
        string splitOn              = "Id",
        CancellationToken ct        = default);

    /// <summary>
    /// Execute a batch of SQL statements that return multiple independent result
    /// sets in one round-trip (Dapper GridReader pattern).
    ///
    /// Use for complex reporting queries — e.g., analytics dashboards that need
    /// summary counts + detail rows + aggregates from a single database call.
    ///
    /// Example:
    /// <code>
    /// var sql = "SELECT COUNT(*) FROM invoices; SELECT * FROM invoices WHERE status = @status;";
    /// await dapper.QueryMultipleAsync(sql, new { status = "Pending" }, async grid =>
    /// {
    ///     var total   = await grid.ReadFirstAsync&lt;int&gt;();
    ///     var pending = (await grid.ReadAsync&lt;Invoice&gt;()).ToList();
    /// });
    /// </code>
    /// </summary>
    Task QueryMultipleAsync(
        string sql,
        Func<Dapper.SqlMapper.GridReader, Task> read,
        object? parameters          = null,
        CancellationToken ct        = default);

    /// <summary>
    /// Execute a batch of SQL statements that return multiple result sets,
    /// returning a value computed by the <paramref name="read"/> callback.
    /// </summary>
    Task<TResult> QueryMultipleAsync<TResult>(
        string sql,
        Func<Dapper.SqlMapper.GridReader, Task<TResult>> read,
        object? parameters          = null,
        CancellationToken ct        = default);
}

