using System.Threading;
using System.Threading.Tasks;

namespace EHRPlatform.Security.RateLimiting;

/// <summary>
/// Interface for rate limiting operations.
/// Single responsibility: Rate limiting contract.
/// </summary>
public interface IRateLimitingService
{
    /// <summary>
    /// Check if request is allowed within rate limit.
    /// </summary>
    Task<bool> IsAllowedAsync(string key, int maxRequests, int windowSeconds, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get current request count for key.
    /// </summary>
    Task<int> GetCountAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Reset rate limit counter for key.
    /// </summary>
    Task ResetAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get remaining allowed requests.
    /// </summary>
    Task<int> GetRemainingAsync(string key, int maxRequests, CancellationToken cancellationToken = default);
}
