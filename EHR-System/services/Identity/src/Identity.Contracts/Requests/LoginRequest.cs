#nullable enable

namespace EHRPlatform.Services.Identity.Contracts.Requests;

/// <summary>
/// Login request DTO.
/// </summary>
public class LoginRequest
{
    /// <summary>
    /// User email address.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// User password (plain text, will be hashed server-side).
    /// </summary>
    public string Password { get; set; } = string.Empty;
}

