#nullable enable

namespace EHRPlatform.Common.Enums;

/// <summary>
/// Encryption status for sensitive data fields.
/// Used across all services to track which data is encrypted.
/// </summary>
public enum EncryptionStatus
{
    /// <summary>Data is not encrypted (plaintext).</summary>
    Unencrypted = 0,

    /// <summary>Data is fully encrypted.</summary>
    Encrypted = 1,

    /// <summary>Data is partially encrypted (some fields only).</summary>
    Partial = 2
}
