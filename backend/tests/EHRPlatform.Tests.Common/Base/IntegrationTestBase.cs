#nullable enable

using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Xunit;
using EHRPlatform.Common.Data.Abstractions;
using EHRPlatform.Common.Data.Contexts;
using EHRPlatform.Tests.Common.Fixtures;
using EHRPlatform.Tests.Common.Helpers;

namespace EHRPlatform.Tests.Common.Base;

/// <summary>
/// Enhanced base class for integration tests with database and fixtures.
/// Provides transaction management for test isolation and clean setup/teardown.
/// </summary>
public abstract class IntegrationTestBase : IAsyncLifetime
{
    protected DatabaseFixture DatabaseFixture { get; private set; } = null!;
    protected CacheFixture CacheFixture { get; private set; } = null!;
    protected DbContextOptions<TestDbContext> DbContextOptions { get; private set; } = null!;
    protected TestDbContext DbContext { get; private set; } = null!;
    protected System.Data.Common.DbTransaction? Transaction { get; private set; }

    /// <summary>
    /// Initialize fixtures and database context.
    /// </summary>
    public virtual async Task InitializeAsync()
    {
        // Initialize database fixture
        DatabaseFixture = new DatabaseFixture();
        await DatabaseFixture.InitializeAsync();

        // Initialize cache fixture
        CacheFixture = new CacheFixture();
        await CacheFixture.InitializeAsync();

        // Setup DbContext options
        DbContextOptions = DatabaseFixture.GetDbContextOptions<TestDbContext>();
        DbContext = new TestDbContext(DbContextOptions);

        // Create tables
        await DbContext.Database.EnsureCreatedAsync();

        // Begin transaction for test isolation
        Transaction = await DbContext.Database.BeginTransactionAsync();
    }

    /// <summary>
    /// Rollback transaction and cleanup.
    /// </summary>
    public virtual async Task DisposeAsync()
    {
        try
        {
            if (Transaction != null)
            {
                await Transaction.RollbackAsync();
                await Transaction.DisposeAsync();
            }

            if (DbContext != null)
            {
                await DbContext.Database.EnsureDeletedAsync();
                await DbContext.DisposeAsync();
            }
        }
        finally
        {
            if (CacheFixture != null)
                await CacheFixture.DisposeAsync();

            if (DatabaseFixture != null)
                await DatabaseFixture.DisposeAsync();
        }
    }

    /// <summary>
    /// Save changes to database during test.
    /// </summary>
    protected async Task SaveChangesAsync()
    {
        await DbContext.SaveChangesAsync();
    }

    /// <summary>
    /// Refresh entity from database.
    /// </summary>
    protected async Task RefreshEntityAsync<T>(T entity) where T : class
    {
        await DbContext.Entry(entity).ReloadAsync();
    }

    /// <summary>
    /// Execute raw SQL against test database.
    /// </summary>
    protected async Task ExecuteSqlAsync(string sql, params object[] parameters)
    {
        await DatabaseFixture.ExecuteSqlAsync(sql, parameters);
    }

    /// <summary>
    /// Query raw SQL from test database.
    /// </summary>
    protected async Task<System.Collections.Generic.List<System.Collections.Generic.Dictionary<string, object>>> QuerySqlAsync(
        string sql, params object[] parameters)
    {
        return await DatabaseFixture.QuerySqlAsync(sql, parameters);
    }

    /// <summary>
    /// Get time elapsed since test started.
    /// </summary>
    protected System.Diagnostics.Stopwatch CreateStopwatch()
    {
        return System.Diagnostics.Stopwatch.StartNew();
    }
}
