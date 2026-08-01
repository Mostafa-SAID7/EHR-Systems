using System.Threading;
using System.Threading.Tasks;

namespace EHRPlatform.Common.FeatureFlags;

/// <summary>
/// Interface for feature flag management.
/// Single responsibility: Feature flag provider contract.
/// </summary>
public interface IFeatureFlagService
{
    /// <summary>
    /// Check if feature is enabled.
    /// </summary>
    Task<bool> IsEnabledAsync(string featureName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if feature is enabled for specific user/context.
    /// </summary>
    Task<bool> IsEnabledAsync(string featureName, string context, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get feature flag value (can be boolean or other type).
    /// </summary>
    Task<T?> GetFeatureValueAsync<T>(string featureName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all feature flags.
    /// </summary>
    Task<Dictionary<string, bool>> GetAllFeaturesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Set feature flag value.
    /// </summary>
    Task SetFeatureFlagAsync(string featureName, bool enabled, CancellationToken cancellationToken = default);

    /// <summary>
    /// Refresh feature flags from source.
    /// </summary>
    Task RefreshAsync(CancellationToken cancellationToken = default);
}
