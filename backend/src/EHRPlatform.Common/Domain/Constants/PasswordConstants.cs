#nullable enable

namespace EHRPlatform.Common.Domain.Constants;

/// <summary>
/// Password hashing configuration constants for PBKDF2-SHA256.
/// Defines security parameters for password storage and verification.
/// Single responsibility: Define password constants only.
/// </summary>
public static class PasswordConstants
{
    /// <summary>Salt size in bytes (128 bits).</summary>
    public const int SaltSizeBytes = 16;

    /// <summary>Hash output size in bytes (256 bits for SHA-256).</summary>
    public const int HashSizeBytes = 32;

    /// <summary>PBKDF2 iteration count (NIST recommended minimum).</summary>
    public const int IterationCount = 10000;

    /// <summary>Hash algorithm name for PBKDF2.</summary>
    public const string HashAlgorithm = "SHA256";

    /// <summary>Format version for password hashes (for future algorithm upgrades).</summary>
    public const int FormatVersion = 1;
}
