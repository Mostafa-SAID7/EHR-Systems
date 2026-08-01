namespace EHRPlatform.Security.Encryption;

/// <summary>
/// Interface for encryption and hashing operations.
/// Single responsibility: Encryption service contract.
/// </summary>
public interface IEncryptionService
{
    /// <summary>
    /// Hash password using bcrypt (one-way, salted).
    /// </summary>
    string HashPassword(string password);

    /// <summary>
    /// Verify password against hash.
    /// </summary>
    bool VerifyPassword(string password, string hash);

    /// <summary>
    /// Generate cryptographically secure random token.
    /// </summary>
    string GenerateSecureToken(int length = 32);

    /// <summary>
    /// Generate OTP (One-Time Password) for MFA.
    /// </summary>
    string GenerateOtp(int length = 6);

    /// <summary>
    /// Encrypt sensitive data (AES-256-GCM).
    /// </summary>
    string EncryptAes256(string plainText, string key);

    /// <summary>
    /// Decrypt AES-256-GCM encrypted data.
    /// </summary>
    string DecryptAes256(string cipherText, string key);
}
