namespace Identity.Contracts.Requests;

/// <summary>
/// Request to authenticate a user
/// </summary>
public sealed record LoginRequest(
    string Email,
    string Password)
{
    /// <summary>
    /// Default constructor for serialization
    /// </summary>
    public LoginRequest() 
        : this(string.Empty, string.Empty)
    {
    }
}
