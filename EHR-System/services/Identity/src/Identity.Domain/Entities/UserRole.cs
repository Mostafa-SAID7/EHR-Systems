namespace EHRPlatform.Services.Identity.Domain.Entities;

/// <summary>
/// User role assignment.
/// </summary>
public class UserRole
{
    public Guid UserId { get; set; }
    public Guid RoleId { get; set; }
    public User User { get; set; } = null!;
    public Role Role { get; set; } = null!;
}

