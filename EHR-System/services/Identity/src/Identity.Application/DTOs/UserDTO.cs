namespace Identity.Application.DTOs;

/// <summary>
/// Data transfer object for user information
/// </summary>
public sealed record UserDTO(
    Guid Id,
    string Email,
    string FirstName,
    string LastName,
    string Status,
    bool IsEmailVerified,
    DateTime CreatedAt,
    DateTime? ModifiedAt = null,
    List<string>? Roles = null);
