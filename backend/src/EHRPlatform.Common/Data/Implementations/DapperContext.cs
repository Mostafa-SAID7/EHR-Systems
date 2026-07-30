#nullable enable

using Dapper;
using EHRPlatform.Common.Data.Abstractions;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace EHRPlatform.Common.Data.Implementations;

/// <summary>
/// Dapper-based implementation of <see cref="IDapperContext"/>.
/// Reuses the EF Core DbContext's underlying <see cref="IDbConnection"/> so
/// all Dapper queries share the same connection and transaction.
/// </summary>
public sealed class DapperContext : IDapperContext
{
    private readonly DbContext _dbContext;

    public DapperContext(DbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    // Ensure the connection is open before Dapper uses it.
    private async Task<IDbConnection> GetOpenConnectionAsync(CancellationToken ct)
    {
        var conn = _dbContext.Database.GetDbConnection();
        if (conn.State != ConnectionState.Open)
            await _dbContext.Database.OpenConnectionAsync(ct);
        return conn;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<T>> QueryAsync<T>(
        string sql,
        object? parameters   = null,
        CancellationToken ct = default)
    {
        var conn = await GetOpenConnectionAsync(ct);
        return await conn.QueryAsync<T>(
            new CommandDefinition(sql, parameters, cancellationToken: ct));
    }

    /// <inheritdoc />
    public async Task<T?> QueryFirstOrDefaultAsync<T>(
        string sql,
        object? parameters   = null,
        CancellationToken ct = default)
    {
        var conn = await GetOpenConnectionAsync(ct);
        return await conn.QueryFirstOrDefaultAsync<T>(
            new CommandDefinition(sql, parameters, cancellationToken: ct));
    }

    /// <inheritdoc />
    public async Task<int> ExecuteAsync(
        string sql,
        object? parameters   = null,
        CancellationToken ct = default)
    {
        var conn = await GetOpenConnectionAsync(ct);
        return await conn.ExecuteAsync(
            new CommandDefinition(sql, parameters, cancellationToken: ct));
    }

    /// <inheritdoc />
    public async Task<T?> ExecuteScalarAsync<T>(
        string sql,
        object? parameters   = null,
        CancellationToken ct = default)
    {
        var conn = await GetOpenConnectionAsync(ct);
        return await conn.ExecuteScalarAsync<T>(
            new CommandDefinition(sql, parameters, cancellationToken: ct));
    }

    /// <inheritdoc />
    public async Task<IEnumerable<TReturn>> QueryAsync<TFirst, TSecond, TReturn>(
        string sql,
        Func<TFirst, TSecond, TReturn> map,
        object? parameters   = null,
        string splitOn       = "Id",
        CancellationToken ct = default)
    {
        var conn = await GetOpenConnectionAsync(ct);
        return await conn.QueryAsync(sql, map, parameters, splitOn: splitOn);
    }

    /// <inheritdoc />
    public async Task QueryMultipleAsync(
        string sql,
        Func<SqlMapper.GridReader, Task> read,
        object? parameters   = null,
        CancellationToken ct = default)
    {
        var conn = await GetOpenConnectionAsync(ct);
        using var grid = await conn.QueryMultipleAsync(
            new CommandDefinition(sql, parameters, cancellationToken: ct));
        await read(grid);
    }

    /// <inheritdoc />
    public async Task<TResult> QueryMultipleAsync<TResult>(
        string sql,
        Func<SqlMapper.GridReader, Task<TResult>> read,
        object? parameters   = null,
        CancellationToken ct = default)
    {
        var conn = await GetOpenConnectionAsync(ct);
        using var grid = await conn.QueryMultipleAsync(
            new CommandDefinition(sql, parameters, cancellationToken: ct));
        return await read(grid);
    }
}

