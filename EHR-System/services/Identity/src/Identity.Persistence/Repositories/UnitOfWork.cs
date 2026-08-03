namespace Identity.Persistence.Repositories;

/// <summary>
/// Implementation of the Unit of Work pattern
/// </summary>
public sealed class UnitOfWork : IUnitOfWork
{
    private readonly IdentityDbContext _dbContext;
    private IUserRepository? _userRepository;
    private IRepository<Role, Guid>? _roleRepository;

    /// <summary>
    /// Initializes a new instance of the UnitOfWork class
    /// </summary>
    /// <param name="dbContext">The database context</param>
    public UnitOfWork(IdentityDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    /// <summary>
    /// Gets the user repository
    /// </summary>
    public IUserRepository Users
    {
        get => _userRepository ??= new UserRepository(_dbContext);
    }

    /// <summary>
    /// Gets the role repository
    /// </summary>
    public IRepository<Role, Guid> Roles
    {
        get => _roleRepository ??= new RoleRepository(_dbContext);
    }

    /// <summary>
    /// Saves all changes to the database
    /// </summary>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>The number of entities changed</returns>
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Begins a transaction
    /// </summary>
    /// <param name="cancellationToken">The cancellation token</param>
    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        await _dbContext.Database.BeginTransactionAsync(cancellationToken);
    }

    /// <summary>
    /// Commits the current transaction
    /// </summary>
    /// <param name="cancellationToken">The cancellation token</param>
    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
            await _dbContext.Database.CommitTransactionAsync(cancellationToken);
        }
        catch
        {
            await RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }

    /// <summary>
    /// Rolls back the current transaction
    /// </summary>
    /// <param name="cancellationToken">The cancellation token</param>
    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await _dbContext.Database.RollbackTransactionAsync(cancellationToken);
        }
        catch
        {
            // Ignore rollback errors
        }
    }

    /// <summary>
    /// Disposes the unit of work
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        await _dbContext.DisposeAsync();
        GC.SuppressFinalize(this);
    }
}
