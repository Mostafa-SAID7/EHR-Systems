namespace Identity.Infrastructure.TwoFactorAuth;

/// <summary>
/// Result of two-factor authentication operation.
/// Single responsibility: 2FA operation result data structure.
/// </summary>
public class TwoFactorResult
{
    /// <summary>
    /// Is operation successful.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Operation message.
    /// </summary>
    public string Message { get; set; } = null!;

    /// <summary>
    /// Secret/code if needed (for setup).
    /// </summary>
    public string? Secret { get; set; }
}
