using System.Threading;
using System.Threading.Tasks;

namespace EHRPlatform.Security.TokenRefresh;

/// <summary>
/// Interface for token refresh operations.
/// Single responsibility: Token refresh contract.
/// </summary>
public interface ITokenRefreshService
{
    /// <summary>
    /// Refresh access token using refresh token.
    /// </summary>
    Task<TokenRefreshResult> RefreshAsync(string refreshToken, CancellationToken cancellationToken = default);

    /// <summary>
    /// Revoke refresh token (logout).
    /// </summary>
    Task RevokeAsync(string refreshToken, CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if refresh token is valid.
    /// </summary>
    Task<bool> IsValidAsync(string refreshToken, CancellationToken cancellationToken = default);
}
