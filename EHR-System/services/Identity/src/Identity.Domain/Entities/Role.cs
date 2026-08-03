namespace Identity.Domain.Entities;

using Identity.Domain.Enums;

/// <summary>
/// Entity representing a role in the system
/// </summary>
public sealed class Role : Entity<Guid>
{
    /// <summary>
    /// Initializes a new instance of the Role class
    /// </summary>
    /// <param name="id">The role ID</param>
    /// <param name="name">The role name</param>
    /// <param name="roleType">The role type</param>
    /// <param name="description">The role description</param>
    private Role(Guid id, string name, RoleType roleType, string description)
        : base(id)
    {
        Name = name;
        RoleType = roleType;
        Description = description;
        CreatedAt = DateTime.UtcNow;
        IsActive = true;
    }

    /// <summary>
    /// Gets the role name
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// Gets the role type
    /// </summary>
    public RoleType RoleType { get; private set; }

    /// <summary>
    /// Gets the role description
    /// </summary>
    public string Description { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the role is active
    /// </summary>
    public bool IsActive { get; private set; }

    /// <summary>
    /// Gets the creation timestamp
    /// </summary>
    public DateTime CreatedAt { get; private set; }

    /// <summary>
    /// Gets the last modification timestamp
    /// </summary>
    public DateTime? ModifiedAt { get; private set; }

    /// <summary>
    /// Creates a new role
    /// </summary>
    /// <param name="name">The role name</param>
    /// <param name="roleType">The role type</param>
    /// <param name="description">The role description</param>
    /// <returns>A new Role instance</returns>
    public static Role Create(string name, RoleType roleType, string description)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Role name is required", nameof(name));

        return new Role(Guid.NewGuid(), name, roleType, description);
    }

    /// <summary>
    /// Deactivates the role
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
        ModifiedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Activates the role
    /// </summary>
    public void Activate()
    {
        IsActive = true;
        ModifiedAt = DateTime.UtcNow;
    }
}
