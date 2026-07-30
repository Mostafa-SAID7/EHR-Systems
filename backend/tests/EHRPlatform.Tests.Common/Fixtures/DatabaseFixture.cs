#nullable enable

using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;
using EHRPlatform.Common.Data.Contexts;

namespace EHRPlatform.Tests.Common.Fixtures;

/// <summary>
/// PostgreSQL Testcontainer fixture for integration and contract tests.
/// Manages full lifecycle: container creation, database setup, cleanup.
/// </summary>
public class DatabaseFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container;
    private string? _connectionString;

    public string ConnectionString
    {
        get => _connectionString ?? throw new InvalidOperationException("Database not initialized");
        private set => _connectionString = value;
    }

    public DbContextOptions<T> GetDbContextOptions<T>() where T : DbContext
    {
        return new DbContextOptionsBuilder<T>()
            .UseNpgsql(ConnectionString)
            .EnableSensitiveDataLogging()
            .Options;
    }

    public DatabaseFixture()
    {
        _container = new PostgreSqlBuilder()
            .WithImage("postgres:16-alpine")
            .WithDatabase("ehr_test")
            .WithUsername("test_user")
            .WithPassword("test_password")
            .WithCleanUp(true)
            .Build();
    }

    /// <summary>
    /// Initialize container and create database schema.
    /// </summary>
    public async Task InitializeAsync()
    {
        await _container.StartAsync();
        ConnectionString = _container.GetConnectionString();

        // Ensure database is ready
        await WaitForDatabaseReadiness();
    }

    /// <summary>
    /// Stop and cleanup container.
    /// </summary>
    public async Task DisposeAsync()
    {
        if (_container != null)
        {
            await _container.StopAsync();
            await _container.DisposeAsync();
        }
    }

    /// <summary>
    /// Wait for PostgreSQL to be ready for connections.
    /// </summary>
    private async Task WaitForDatabaseReadiness(int maxRetries = 10)
    {
        int retries = 0;
        while (retries < maxRetries)
        {
            try
            {
                using var connection = new Npgsql.NpgsqlConnection(ConnectionString);
                await connection.OpenAsync();
                await connection.CloseAsync();
                return;
            }
            catch
            {
                retries++;
                if (retries >= maxRetries)
                    throw;
                await Task.Delay(500);
            }
        }
    }

    /// <summary>
    /// Reset all tables (truncate) for test isolation.
    /// </summary>
    public async Task ResetDatabaseAsync(string[]? tablesToKeep = null)
    {
        using var connection = new Npgsql.NpgsqlConnection(ConnectionString);
        await connection.OpenAsync();

        using var cmd = connection.CreateCommand();
        cmd.CommandText = @"
            SELECT table_name FROM information_schema.tables 
            WHERE table_schema = 'public' AND table_type = 'BASE TABLE'";

        var tables = new List<string>();
        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            tables.Add(reader.GetString(0));
        }

        foreach (var table in tables)
        {
            if (tablesToKeep?.Contains(table) == true)
                continue;

            using var truncateCmd = connection.CreateCommand();
            truncateCmd.CommandText = $"TRUNCATE TABLE {table} CASCADE";
            await truncateCmd.ExecuteNonQueryAsync();
        }
    }

    /// <summary>
    /// Execute raw SQL against the test database.
    /// </summary>
    public async Task<int> ExecuteSqlAsync(string sql, params object[] parameters)
    {
        using var connection = new Npgsql.NpgsqlConnection(ConnectionString);
        await connection.OpenAsync();

        using var cmd = connection.CreateCommand();
        cmd.CommandText = sql;

        if (parameters.Length > 0)
        {
            for (int i = 0; i < parameters.Length; i++)
            {
                cmd.Parameters.AddWithValue($"p{i}", parameters[i] ?? DBNull.Value);
            }
        }

        return await cmd.ExecuteNonQueryAsync();
    }

    /// <summary>
    /// Query raw SQL from the test database.
    /// </summary>
    public async Task<List<Dictionary<string, object>>> QuerySqlAsync(string sql, params object[] parameters)
    {
        var result = new List<Dictionary<string, object>>();

        using var connection = new Npgsql.NpgsqlConnection(ConnectionString);
        await connection.OpenAsync();

        using var cmd = connection.CreateCommand();
        cmd.CommandText = sql;

        if (parameters.Length > 0)
        {
            for (int i = 0; i < parameters.Length; i++)
            {
                cmd.Parameters.AddWithValue($"p{i}", parameters[i] ?? DBNull.Value);
            }
        }

        using var reader = await cmd.ExecuteReaderAsync();
        var fieldCount = reader.FieldCount;

        while (await reader.ReadAsync())
        {
            var row = new Dictionary<string, object>();
            for (int i = 0; i < fieldCount; i++)
            {
                row[reader.GetName(i)] = reader.GetValue(i) ?? DBNull.Value;
            }
            result.Add(row);
        }

        return result;
    }
}
