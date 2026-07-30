namespace EHRPlatform.Common.Infrastructure.Security;

/// <summary>
/// Password hashing service for user authentication.
/// Uses PBKDF2-SHA256 with high iteration count.
/// </summary>
public interface IPasswordHasher
{
    /// <summary>
    /// Hash a plaintext password — salt is embedded in the returned string.
    /// Suitable for single-column storage.
    /// </summary>
    string HashPassword(string password);

    /// <summary>
    /// Verify a plaintext password against a hash that has an embedded salt.
    /// </summary>
    bool VerifyPassword(string password, string hash);

    /// <summary>
    /// Hash a password and return hash and salt as separate strings.
    /// Use when the schema stores them in two distinct columns.
    /// </summary>
    (string hash, string salt) HashWithSalt(string password);

    /// <summary>
    /// Verify a plaintext password against an explicitly provided hash and salt.
    /// </summary>
    bool Verify(string password, string hash, string salt);

    /// <summary>
    /// Produce a deterministic hash of <paramref name="value"/> using the given
    /// <paramref name="salt"/>. Passing an empty salt is allowed (e.g. for refresh tokens).
    /// </summary>
    string Hash(string value, string salt);
}

