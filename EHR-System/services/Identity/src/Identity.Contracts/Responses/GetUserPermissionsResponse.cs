#nullable enable

namespace EHRPlatform.Services.Identity.Contracts.Responses;

/// <summary>
/// Get user permissions response.
/// Contains flattened list of all permissions for the user.
/// </summary>
public class GetUserPermissionsResponse
{
    /// <summary>
    /// User ID.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// List of permission strings in format "resource:action".
    /// Example: ["patient:read", "patient:write", "appointment:read"]
    /// </summary>
    public List<string> Permissions { get; set; } = new();

    /// <summary>
    /// User roles.
    /// </summary>
    public List<string> Roles { get; set; } = new();
}

