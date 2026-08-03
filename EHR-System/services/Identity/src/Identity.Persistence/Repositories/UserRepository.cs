namespace Identity.Persistence.Repositories;

/// <summary>
/// Repository implementation for User entities
/// </summary>
public sealed class UserRepository : RepositoryBase<User, Guid>, IUserRepository
{
    /// <summary>
    /// Initializes a new instance of the UserRepository class
    /// </summary>
    /// <param name="dbContext">The database context</param>
    public UserRepository(IdentityDbContext dbContext)
        : base(dbContext)
    {
    }

    /// <summary>
    /// Gets a user by email address
    /// </summary>
    /// <param name="email">The email address</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>The user if found; otherwise null</returns>
    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await DbSet.FirstOrDefaultAsync(u => u.Email.Value == email, cancellationToken);
    }

    /// <summary>
    /// Checks if a user with the given email exists
    /// </summary>
    /// <param name="email">The email address</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>True if the user exists; otherwise false</returns>
    public async Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await DbSet.AnyAsync(u => u.Email.Value == email, cancellationToken);
    }

    /// <summary>
    /// Gets all active users
    /// </summary>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>A collection of active users</returns>
    public async Task<IEnumerable<User>> GetAllActiveAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(u => u.IsActive)
            .Include(u => u.Roles)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Gets users by role
    /// </summary>
    /// <param name="roleId">The role ID</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>A collection of users with the specified role</returns>
    public async Task<IEnumerable<User>> GetByRoleAsync(Guid roleId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(u => u.Roles.Any(ur => ur.RoleId == roleId))
            .Include(u => u.Roles)
            .ToListAsync(cancellationToken);
    }
}
