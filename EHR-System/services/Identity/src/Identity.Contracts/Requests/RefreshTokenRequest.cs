namespace Identity.Contracts.Requests;

/// <summary>
/// Request to refresh an access token
/// </summary>
public sealed record RefreshTokenRequest(
    string RefreshToken)
{
    /// <summary>
    /// Default constructor for serialization
    /// </summary>
    public RefreshTokenRequest() 
        : this(string.Empty)
    {
    }
}
