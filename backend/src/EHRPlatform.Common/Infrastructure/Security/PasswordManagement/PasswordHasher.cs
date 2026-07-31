using EHRPlatform.Common.Domain.Constants;
using EHRPlatform.Common.Domain.Constants;
using System.Security.Cryptography;
using System.Text;
using EHRPlatform.Common.Shared.Utilities.Helpers;

namespace EHRPlatform.Common.Infrastructure.Security;

/// <summary>
/// PBKDF2-SHA256 password hashing implementation.
/// Designed for user password storage with high security.
/// </summary>
public class PasswordHasher : IPasswordHasher
{
    // ── Single-column (embedded-salt) API ────────────────────────────────────

    public string HashPassword(string password)
    {
        if (StringHelper.IsNullOrEmpty(password))
            throw new ArgumentException("Password cannot be empty", nameof(password));

        var salt = RandomNumberGenerator.GetBytes(PasswordConstants.SaltSizeBytes);
        var hash = Pbkdf2(password, salt);

        var result = new byte[PasswordConstants.SaltSizeBytes + PasswordConstants.HashSizeBytes];
        Buffer.BlockCopy(salt, 0, result, 0,             PasswordConstants.SaltSizeBytes);
        Buffer.BlockCopy(hash, 0, result, PasswordConstants.SaltSizeBytes, PasswordConstants.HashSizeBytes);
        return ConversionHelper.ToBase64(result);
    }

    public bool VerifyPassword(string password, string storedHash)
    {
        if (StringHelper.IsNullOrEmpty(password) || StringHelper.IsNullOrEmpty(storedHash))
            return false;

        try
        {
            var data = ConversionHelper.FromBase64Bytes(storedHash);
            if (data == null || data.Length < SaltSizeBytes + HashSizeBytes) return false;

            var salt   = data[..SaltSizeBytes];
            var stored = data[SaltSizeBytes..];
            var computed = Pbkdf2(password, salt);
            return ConstantTimeEquals(computed, stored);
        }
        catch { return false; }
    }

    // ── Two-column (separate hash + salt) API ────────────────────────────────

    public (string hash, string salt) HashWithSalt(string password)
    {
        if (StringHelper.IsNullOrEmpty(password))
            throw new ArgumentException("Password cannot be empty", nameof(password));

        var saltBytes = RandomNumberGenerator.GetBytes(SaltSizeBytes);
        var hashBytes = Pbkdf2(password, saltBytes);

        return (ConversionHelper.ToBase64(hashBytes), ConversionHelper.ToBase64(saltBytes));
    }

    public bool Verify(string password, string hash, string salt)
    {
        if (StringHelper.IsNullOrEmpty(password)) return false;

        try
        {
            var saltBytes     = StringHelper.IsNullOrEmpty(salt) ? Array.Empty<byte>() : ConversionHelper.FromBase64Bytes(salt) ?? Array.Empty<byte>();
            var storedHash    = ConversionHelper.FromBase64Bytes(hash) ?? Array.Empty<byte>();
            var computedHash  = Pbkdf2(password, saltBytes);
            return ConstantTimeEquals(computedHash, storedHash);
        }
        catch { return false; }
    }

    // ── Generic hash (for tokens, etc.) ──────────────────────────────────────

    public string Hash(string value, string salt)
    {
        var saltBytes = StringHelper.IsNullOrEmpty(salt)
            ? Array.Empty<byte>()
            : ConversionHelper.FromBase64Bytes(salt) ?? Array.Empty<byte>();

        var hashBytes = Pbkdf2(value, saltBytes);
        return ConversionHelper.ToBase64(hashBytes);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static byte[] Pbkdf2(string password, byte[] salt)
    {
        using var pbkdf2 = new Rfc2898DeriveBytes(
            password, salt, IterationCount, HashAlgorithmName.SHA256);
        return pbkdf2.GetBytes(HashSizeBytes);
    }

    private static bool ConstantTimeEquals(byte[] a, byte[] b)
    {
        if (a.Length != b.Length) return false;
        int diff = 0;
        for (int i = 0; i < a.Length; i++) diff |= a[i] ^ b[i];
        return diff == 0;
    }
}

