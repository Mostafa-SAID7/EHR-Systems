namespace Identity.Contracts.Responses;

/// <summary>
/// Response containing authentication tokens
/// </summary>
public sealed record AuthResponse(
    string AccessToken,
    string RefreshToken,
    int ExpiresIn,
    string TokenType = "Bearer")
{
    /// <summary>
    /// Default constructor for serialization
    /// </summary>
    public AuthResponse() 
        : this(string.Empty, string.Empty, 0)
    {
    }
}
