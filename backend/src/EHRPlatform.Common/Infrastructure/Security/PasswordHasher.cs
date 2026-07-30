using System.Security.Cryptography;
using System.Text;

namespace EHRPlatform.Common.Infrastructure.Security;

/// <summary>
/// PBKDF2-SHA256 password hashing implementation.
/// Designed for user password storage with high security.
/// </summary>
public class PasswordHasher : IPasswordHasher
{
    private const int SaltSizeBytes  = 16;    // 128 bits
    private const int HashSizeBytes  = 32;    // 256 bits (SHA-256 output)
    private const int IterationCount = 10000; // NIST recommended minimum

    // ── Single-column (embedded-salt) API ────────────────────────────────────

    public string HashPassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("Password cannot be empty", nameof(password));

        var salt = RandomNumberGenerator.GetBytes(SaltSizeBytes);
        var hash = Pbkdf2(password, salt);

        var result = new byte[SaltSizeBytes + HashSizeBytes];
        Buffer.BlockCopy(salt, 0, result, 0,             SaltSizeBytes);
        Buffer.BlockCopy(hash, 0, result, SaltSizeBytes, HashSizeBytes);
        return Convert.ToBase64String(result);
    }

    public bool VerifyPassword(string password, string storedHash)
    {
        if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(storedHash))
            return false;

        try
        {
            var data = Convert.FromBase64String(storedHash);
            if (data.Length < SaltSizeBytes + HashSizeBytes) return false;

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
        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("Password cannot be empty", nameof(password));

        var saltBytes = RandomNumberGenerator.GetBytes(SaltSizeBytes);
        var hashBytes = Pbkdf2(password, saltBytes);

        return (Convert.ToBase64String(hashBytes), Convert.ToBase64String(saltBytes));
    }

    public bool Verify(string password, string hash, string salt)
    {
        if (string.IsNullOrWhiteSpace(password)) return false;

        try
        {
            var saltBytes     = string.IsNullOrEmpty(salt) ? Array.Empty<byte>() : Convert.FromBase64String(salt);
            var storedHash    = Convert.FromBase64String(hash);
            var computedHash  = Pbkdf2(password, saltBytes);
            return ConstantTimeEquals(computedHash, storedHash);
        }
        catch { return false; }
    }

    // ── Generic hash (for tokens, etc.) ──────────────────────────────────────

    public string Hash(string value, string salt)
    {
        var saltBytes = string.IsNullOrEmpty(salt)
            ? Array.Empty<byte>()
            : Convert.FromBase64String(salt);

        var hashBytes = Pbkdf2(value, saltBytes);
        return Convert.ToBase64String(hashBytes);
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

