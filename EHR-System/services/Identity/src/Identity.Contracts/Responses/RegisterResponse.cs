#nullable enable

namespace EHRPlatform.Services.Identity.Contracts.Responses;

/// <summary>
/// User registration response DTO.
/// </summary>
public class RegisterResponse
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
    /// Success message.
    /// </summary>
    public string Message { get; set; } = "User registered successfully";
}

