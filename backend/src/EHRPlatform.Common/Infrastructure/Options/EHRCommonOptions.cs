#nullable enable

namespace EHRPlatform.Common.Infrastructure.Options;

/// <summary>
/// Configuration options for EHR Common infrastructure services.
/// Single responsibility: Hold configuration for logging, caching, and encryption.
/// </summary>
public class EHRCommonOptions
{
    /// <summary>Redis connection string (e.g., "localhost:6379,password=secret").</summary>
    public string RedisConnectionString { get; set; } = "localhost:6379";

    /// <summary>Encryption key for sensitive data (must be 32+ characters).</summary>
    public string EncryptionKey { get; set; } = string.Empty;

    /// <summary>Enable or disable Redis caching (default: true).</summary>
    public bool EnableCaching { get; set; } = true;

    /// <summary>Enable or disable encryption (default: true).</summary>
    public bool EnableEncryption { get; set; } = true;

    /// <summary>Enable or disable Serilog logging (default: true).</summary>
    public bool EnableLogging { get; set; } = true;
}
