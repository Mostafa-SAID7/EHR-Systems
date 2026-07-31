#nullable enable

namespace EHRPlatform.Common.Domain.Constants;

/// <summary>
/// Encryption configuration constants for AES-GCM.
/// Defines key sizes, nonce sizes, and security parameters.
/// Single responsibility: Define encryption constants only.
/// </summary>
public static class EncryptionConstants
{
    /// <summary>Nonce size in bytes (96 bits for GCM).</summary>
    public const int NonceSizeBytes = 12;

    /// <summary>Authentication tag size in bytes (128 bits).</summary>
    public const int TagSizeBytes = 16;

    /// <summary>Salt size in bytes (128 bits).</summary>
    public const int SaltSizeBytes = 16;

    /// <summary>Key size in bytes (256 bits for AES-256).</summary>
    public const int KeySizeBytes = 32;

    /// <summary>PBKDF2 iteration count (NIST recommended minimum).</summary>
    public const int Pbkdf2Iterations = 10000;

    /// <summary>PBKDF2 hash algorithm name.</summary>
    public const string Pbkdf2Algorithm = "SHA256";
}
