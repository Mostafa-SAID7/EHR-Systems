using System.Threading;
using System.Threading.Tasks;

namespace EHRPlatform.Security.TwoFactorAuth;

/// <summary>
/// Interface for two-factor authentication services.
/// Single responsibility: 2FA authentication contract.
/// </summary>
public interface ITwoFactorAuthService
{
    /// <summary>
    /// Generate 2FA OTP and send to user.
    /// </summary>
    Task<TwoFactorResult> GenerateAndSendOtpAsync(string userId, string destination, CancellationToken cancellationToken = default);

    /// <summary>
    /// Verify OTP is correct.
    /// </summary>
    Task<bool> VerifyOtpAsync(string userId, string otp, CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if user has 2FA enabled.
    /// </summary>
    Task<bool> IsEnabledForUserAsync(string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Enable 2FA for user.
    /// </summary>
    Task<TwoFactorResult> EnableAsync(string userId, string method, CancellationToken cancellationToken = default);

    /// <summary>
    /// Disable 2FA for user.
    /// </summary>
    Task DisableAsync(string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Generate backup codes for user.
    /// </summary>
    Task<string[]> GenerateBackupCodesAsync(string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Verify backup code (one-time use).
    /// </summary>
    Task<bool> VerifyBackupCodeAsync(string userId, string backupCode, CancellationToken cancellationToken = default);
}
