namespace Identity.Contracts.Responses;

/// <summary>
/// Response containing user information
/// </summary>
public sealed record UserResponse(
    Guid Id,
    string Email,
    string FirstName,
    string LastName,
    string Status,
    bool IsEmailVerified,
    DateTime CreatedAt,
    DateTime? ModifiedAt = null,
    List<string>? Roles = null)
{
    /// <summary>
    /// Default constructor for serialization
    /// </summary>
    public UserResponse() 
        : this(Guid.Empty, string.Empty, string.Empty, string.Empty, string.Empty, false, DateTime.UtcNow)
    {
    }
}
