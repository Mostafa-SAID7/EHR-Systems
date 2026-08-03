#nullable enable

namespace Identity.Contracts.Responses;

/// <summary>
/// MFA setup response DTO.
/// Contains TOTP secret and backup codes.
/// </summary>
public class SetupMfaResponse
{
    /// <summary>
    /// TOTP secret key (can be used to generate QR code).
    /// </summary>
    public string Secret { get; set; } = string.Empty;

    /// <summary>
    /// QR code data URL for scanning with authenticator app.
    /// </summary>
    public string QrCodeUrl { get; set; } = string.Empty;

    /// <summary>
    /// Backup codes for account recovery (one-time use).
    /// </summary>
    public List<string> BackupCodes { get; set; } = new();

    /// <summary>
    /// Message to display to user.
    /// </summary>
    public string Message { get; set; } = "MFA setup successful. Scan the QR code or enter the secret manually.";
}

