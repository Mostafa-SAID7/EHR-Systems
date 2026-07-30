using EHRPlatform.Common.Domain.Entities;

namespace EHRPlatform.Services.Identity.Domain.Entities;

/// <summary>
/// Role with permissions for RBAC.
/// </summary>
public class Role : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public ICollection<UserRole> Users { get; } = new List<UserRole>();
    public ICollection<RolePermission> Permissions { get; } = new List<RolePermission>();
}

