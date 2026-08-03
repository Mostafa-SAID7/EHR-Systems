#nullable enable

namespace Identity.Contracts.Requests;

/// <summary>
/// User registration request DTO.
/// </summary>
public class RegisterRequest
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
    /// Password (plain text, will be hashed server-side).
    /// </summary>
    public string Password { get; set; } = string.Empty;
}

