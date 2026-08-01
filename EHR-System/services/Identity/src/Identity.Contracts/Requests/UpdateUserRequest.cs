#nullable enable

namespace EHRPlatform.Services.Identity.Contracts.Requests;

/// <summary>
/// Update user request DTO.
/// </summary>
public class UpdateUserRequest
{
    /// <summary>
    /// First name.
    /// </summary>
    public string? FirstName { get; set; }

    /// <summary>
    /// Last name.
    /// </summary>
    public string? LastName { get; set; }

    /// <summary>
    /// Email address.
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// Whether the user is active.
    /// </summary>
    public bool? IsActive { get; set; }
}

