namespace Identity.Domain.Entities;

/// <summary>
/// Entity representing the relationship between users and roles
/// </summary>
public sealed class UserRole : Entity<Guid>
{
    /// <summary>
    /// Initializes a new instance of the UserRole class
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="roleId">The role ID</param>
    public UserRole(Guid userId, Guid roleId)
        : base(Guid.NewGuid())
    {
        UserId = userId;
        RoleId = roleId;
        AssignedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Gets the user ID
    /// </summary>
    public Guid UserId { get; private set; }

    /// <summary>
    /// Gets the role ID
    /// </summary>
    public Guid RoleId { get; private set; }

    /// <summary>
    /// Gets the assignment timestamp
    /// </summary>
    public DateTime AssignedAt { get; private set; }

    /// <summary>
    /// Gets or sets the user navigation property
    /// </summary>
    public User? User { get; set; }

    /// <summary>
    /// Gets or sets the role navigation property
    /// </summary>
    public Role? Role { get; set; }
}
