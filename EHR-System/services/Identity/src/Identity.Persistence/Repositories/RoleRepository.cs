namespace Identity.Persistence.Repositories;

using Identity.Domain.Enums;

/// <summary>
/// Repository implementation for Role entities
/// </summary>
public sealed class RoleRepository : RepositoryBase<Role, Guid>, IRoleRepository
{
    /// <summary>
    /// Initializes a new instance of the RoleRepository class
    /// </summary>
    /// <param name="dbContext">The database context</param>
    public RoleRepository(IdentityDbContext dbContext)
        : base(dbContext)
    {
    }

    /// <summary>
    /// Gets a role by name
    /// </summary>
    /// <param name="name">The role name</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>The role if found; otherwise null</returns>
    public async Task<Role?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await DbSet.FirstOrDefaultAsync(r => r.Name == name, cancellationToken);
    }

    /// <summary>
    /// Gets a role by type
    /// </summary>
    /// <param name="roleType">The role type</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>The role if found; otherwise null</returns>
    public async Task<Role?> GetByTypeAsync(RoleType roleType, CancellationToken cancellationToken = default)
    {
        return await DbSet.FirstOrDefaultAsync(r => r.RoleType == roleType, cancellationToken);
    }

    /// <summary>
    /// Gets all active roles
    /// </summary>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>A collection of active roles</returns>
    public async Task<IEnumerable<Role>> GetAllActiveAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(r => r.IsActive)
            .ToListAsync(cancellationToken);
    }
}
