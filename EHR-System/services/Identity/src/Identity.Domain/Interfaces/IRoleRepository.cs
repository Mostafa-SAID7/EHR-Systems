namespace Identity.Domain.Interfaces;

using Identity.Domain.Entities;
using Identity.Domain.Enums;

/// <summary>
/// Repository interface for Role entity
/// </summary>
public interface IRoleRepository : IRepository<Role, Guid>
{
    /// <summary>
    /// Gets a role by name
    /// </summary>
    /// <param name="name">The role name</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>The role if found; otherwise null</returns>
    Task<Role?> GetByNameAsync(string name, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a role by type
    /// </summary>
    /// <param name="roleType">The role type</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>The role if found; otherwise null</returns>
    Task<Role?> GetByTypeAsync(RoleType roleType, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all active roles
    /// </summary>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>A collection of active roles</returns>
    Task<IEnumerable<Role>> GetAllActiveAsync(CancellationToken cancellationToken = default);
}
