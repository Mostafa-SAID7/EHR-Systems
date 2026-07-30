using EHRPlatform.Common.Domain.Entities;

namespace EHRPlatform.Services.Identity.Domain.Entities;

/// <summary>
/// Fine-grained permission for RBAC.
/// Convention: Name = "{Resource}:{Action}" (e.g. "patient:read", "prescription:write").
/// </summary>
public class Permission : BaseEntity
{
    public string Name        { get; set; } = string.Empty;
    public string Resource    { get; set; } = string.Empty;
    public string Action      { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    // ── Navigation ───────────────────────────────────────────────────────────
    public ICollection<RolePermission> Roles { get; } = new List<RolePermission>();

    // ── Helpers ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Validates that <see cref="Resource"/> and <see cref="Action"/> are non-empty
    /// and that <see cref="Name"/> follows the "resource:action" convention.
    /// </summary>
    public bool IsValidFormat() =>
        !string.IsNullOrWhiteSpace(Resource)
        && !string.IsNullOrWhiteSpace(Action)
        && Name == $"{Resource.ToLowerInvariant()}:{Action.ToLowerInvariant()}";

    /// <summary>
    /// Factory: builds a <see cref="Permission"/> from resource + action, ensuring
    /// <see cref="Name"/> is always consistent.
    /// </summary>
    public static Permission Create(string resource, string action, string description = "") =>
        new()
        {
            Resource    = resource.ToLowerInvariant(),
            Action      = action.ToLowerInvariant(),
            Name        = $"{resource.ToLowerInvariant()}:{action.ToLowerInvariant()}",
            Description = description
        };
}

