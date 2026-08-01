using System;
using System.Security.Cryptography;
using System.Text;

namespace EHRPlatform.Security.Encryption;

/// <summary>
/// Service for password hashing and verification using bcrypt.
/// </summary>
public class EncryptionService
{
    /// <summary>
    /// Hash password using bcrypt (one-way, salted).
    /// </summary>
    public static string HashPassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("Password cannot be empty", nameof(password));

        // bcrypt with work factor 12
        return BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);
    }

    /// <summary>
    /// Verify password against hash.
    /// </summary>
    public static bool VerifyPassword(string password, string hash)
    {
        if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(hash))
            return false;

        try
        {
            return BCrypt.Net.BCrypt.Verify(password, hash);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Generate cryptographically secure random token.
    /// </summary>
    public static string GenerateSecureToken(int length = 32)
    {
        using var rng = new RNGCryptoServiceProvider();
        var tokenData = new byte[length];
        rng.GetBytes(tokenData);
        return Convert.ToBase64String(tokenData);
    }

    /// <summary>
    /// Generate OTP (One-Time Password) for MFA.
    /// </summary>
    public static string GenerateOtp(int length = 6)
    {
        using var rng = new RNGCryptoServiceProvider();
        var tokenData = new byte[length];
        rng.GetBytes(tokenData);

        // Convert to numeric string
        var otp = new StringBuilder();
        foreach (var b in tokenData)
        {
            otp.Append(b % 10);
        }

        return otp.ToString()[..length];
    }

    /// <summary>
    /// Encrypt sensitive data (AES-256-GCM).
    /// </summary>
    public static string EncryptAes256(string plainText, string key)
    {
        if (string.IsNullOrEmpty(plainText) || string.IsNullOrEmpty(key))
            throw new ArgumentException("Plain text and key cannot be empty");

        using var aes = Aes.Create();
        aes.Mode = CipherMode.GCM;
        aes.Padding = PaddingMode.PKCS7;
        aes.Key = Encoding.UTF8.GetBytes(key);

        using var encryptor = aes.CreateEncryptor();
        var plainBytes = Encoding.UTF8.GetBytes(plainText);
        var encryptedBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

        // Combine IV + encrypted data
        var result = new byte[aes.IV.Length + encryptedBytes.Length];
        Buffer.BlockCopy(aes.IV, 0, result, 0, aes.IV.Length);
        Buffer.BlockCopy(encryptedBytes, 0, result, aes.IV.Length, encryptedBytes.Length);

        return Convert.ToBase64String(result);
    }

    /// <summary>
    /// Decrypt AES-256-GCM encrypted data.
    /// </summary>
    public static string DecryptAes256(string cipherText, string key)
    {
        if (string.IsNullOrEmpty(cipherText) || string.IsNullOrEmpty(key))
            throw new ArgumentException("Cipher text and key cannot be empty");

        using var aes = Aes.Create();
        aes.Mode = CipherMode.GCM;
        aes.Padding = PaddingMode.PKCS7;
        aes.Key = Encoding.UTF8.GetBytes(key);

        var buffer = Convert.FromBase64String(cipherText);

        // Extract IV
        aes.IV = new byte[aes.IV.Length];
        Buffer.BlockCopy(buffer, 0, aes.IV, 0, aes.IV.Length);

        using var decryptor = aes.CreateDecryptor();
        var decryptedBytes = decryptor.TransformFinalBlock(buffer, aes.IV.Length, buffer.Length - aes.IV.Length);

        return Encoding.UTF8.GetString(decryptedBytes);
    }
}
