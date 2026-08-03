namespace Identity.Infrastructure.TokenRefresh;

/// <summary>
/// Result of token refresh operation.
/// Single responsibility: Token refresh result data structure.
/// </summary>
public class TokenRefreshResult
{
    /// <summary>
    /// New access token.
    /// </summary>
    public string? AccessToken { get; set; }

    /// <summary>
    /// New refresh token (if rotated).
    /// </summary>
    public string? RefreshToken { get; set; }

    /// <summary>
    /// Is refresh successful.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Error message if failed.
    /// </summary>
    public string? Error { get; set; }
}
