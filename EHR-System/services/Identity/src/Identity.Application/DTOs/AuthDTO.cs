namespace Identity.Application.DTOs;

/// <summary>
/// Data transfer object for authentication response
/// </summary>
public sealed record AuthDTO(
    string AccessToken,
    string RefreshToken,
    int ExpiresIn,
    UserDTO User,
    string TokenType = "Bearer");
