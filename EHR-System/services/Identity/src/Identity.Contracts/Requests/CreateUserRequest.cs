#nullable enable

namespace EHRPlatform.Services.Identity.Contracts.Requests;

/// <summary>
/// Create user request DTO (admin-only).
/// </summary>
public class CreateUserRequest
{
    /// <summary>
    /// Email address.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// First name.
    /// </summary>
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// Last name.
    /// </summary>
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// Role name to assign.
    /// </summary>
    public string Role { get; set; } = string.Empty;

    /// <summary>
    /// Department (optional).
    /// </summary>
    public string? Department { get; set; }
}

