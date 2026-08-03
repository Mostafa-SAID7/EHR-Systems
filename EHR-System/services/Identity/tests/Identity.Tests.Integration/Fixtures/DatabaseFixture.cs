namespace Identity.Tests.Integration.Fixtures;

using Identity.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// Fixture for database setup and teardown in integration tests
/// </summary>
public sealed class DatabaseFixture : IAsyncLifetime
{
    private readonly DbContextOptions<IdentityDbContext> _dbContextOptions;
    public IdentityDbContext DbContext { get; private set; } = null!;

    /// <summary>
    /// Initializes a new instance of the DatabaseFixture class
    /// </summary>
    public DatabaseFixture()
    {
        _dbContextOptions = new DbContextOptionsBuilder<IdentityDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
    }

    /// <summary>
    /// Initializes the fixture asynchronously
    /// </summary>
    public async Task InitializeAsync()
    {
        DbContext = new IdentityDbContext(_dbContextOptions);
        await DbContext.Database.EnsureCreatedAsync();
    }

    /// <summary>
    /// Disposes the fixture asynchronously
    /// </summary>
    public async Task DisposeAsync()
    {
        if (DbContext != null)
        {
            await DbContext.Database.EnsureDeletedAsync();
            await DbContext.DisposeAsync();
        }
    }

    /// <summary>
    /// Clears all data from the database
    /// </summary>
    public async Task ClearAsync()
    {
        DbContext.Users.RemoveRange(DbContext.Users);
        DbContext.Roles.RemoveRange(DbContext.Roles);
        DbContext.UserRoles.RemoveRange(DbContext.UserRoles);
        await DbContext.SaveChangesAsync();
    }
}
