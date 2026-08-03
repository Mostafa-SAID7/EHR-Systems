#nullable enable

namespace Identity.Contracts.Responses;

/// <summary>
/// Create user response DTO.
/// </summary>
public class CreateUserResponse
{
    /// <summary>
    /// Newly created user ID.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// User email address.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Temporarily generated password (user should change on first login).
    /// </summary>
    public string TemporaryPassword { get; set; } = string.Empty;

    /// <summary>
    /// Success message.
    /// </summary>
    public string Message { get; set; } = "User created successfully";
}

