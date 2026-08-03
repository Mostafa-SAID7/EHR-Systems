namespace Identity.Contracts.Requests;

/// <summary>
/// Request to create a new user
/// </summary>
public sealed record CreateUserRequest(
    string Email,
    string FirstName,
    string LastName,
    string Password,
    string? PhoneNumber = null,
    List<string>? RoleIds = null)
{
    /// <summary>
    /// Default constructor for serialization
    /// </summary>
    public CreateUserRequest() 
        : this(string.Empty, string.Empty, string.Empty, string.Empty)
    {
    }
}
