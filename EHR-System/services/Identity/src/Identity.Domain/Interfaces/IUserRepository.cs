namespace Identity.Domain.Interfaces;

using Identity.Domain.Entities;

/// <summary>
/// Repository interface for User entity
/// </summary>
public interface IUserRepository : IRepository<User, Guid>
{
    /// <summary>
    /// Gets a user by email address
    /// </summary>
    /// <param name="email">The email address</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>The user if found; otherwise null</returns>
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a user with the given email exists
    /// </summary>
    /// <param name="email">The email address</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>True if the user exists; otherwise false</returns>
    Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all active users
    /// </summary>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>A collection of active users</returns>
    Task<IEnumerable<User>> GetAllActiveAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets users by role
    /// </summary>
    /// <param name="roleId">The role ID</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>A collection of users with the specified role</returns>
    Task<IEnumerable<User>> GetByRoleAsync(Guid roleId, CancellationToken cancellationToken = default);
}
