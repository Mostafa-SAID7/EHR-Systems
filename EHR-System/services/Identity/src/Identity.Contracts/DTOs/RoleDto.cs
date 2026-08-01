#nullable enable

namespace EHRPlatform.Services.Identity.Contracts.DTOs.Responses;

/// <summary>
/// Role data transfer object.
/// </summary>
public class RoleDto
{
    /// <summary>
    /// Role ID.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Role name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Role description.
    /// </summary>
    public string Description { get; set; } = string.Empty;
}

