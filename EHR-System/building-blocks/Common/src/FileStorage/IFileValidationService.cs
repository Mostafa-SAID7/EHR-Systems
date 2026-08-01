using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace EHRPlatform.Common.FileStorage;

/// <summary>
/// Interface for file validation (type, size, content, security).
/// Single responsibility: Validate uploaded files.
/// </summary>
public interface IFileValidationService
{
    /// <summary>
    /// Validate file type.
    /// </summary>
    Task<bool> ValidateFileTypeAsync(string fileName, string contentType, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validate file size.
    /// </summary>
    Task<bool> ValidateFileSizeAsync(long fileSizeBytes, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validate file content against type.
    /// </summary>
    Task<bool> ValidateFileContentAsync(Stream fileStream, string contentType, CancellationToken cancellationToken = default);

    /// <summary>
    /// Scan file for malware/viruses.
    /// </summary>
    Task<FileSecurityResult> ScanForMalwareAsync(Stream fileStream, string fileName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get file hash for integrity checking.
    /// </summary>
    Task<string> GetFileHashAsync(Stream fileStream, CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if file type is allowed.
    /// </summary>
    Task<bool> IsAllowedFileTypeAsync(string contentType, CancellationToken cancellationToken = default);
}

/// <summary>
/// File security scan result.
/// Single responsibility: Security scan data.
/// </summary>
public class FileSecurityResult
{
    /// <summary>
    /// Is file safe.
    /// </summary>
    public bool IsSafe { get; set; }

    /// <summary>
    /// Threat level (if found).
    /// </summary>
    public ThreatLevel ThreatLevel { get; set; }

    /// <summary>
    /// Details of threats (if any).
    /// </summary>
    public string? Details { get; set; }

    /// <summary>
    /// Scan engine name.
    /// </summary>
    public string? ScanEngine { get; set; }
}

/// <summary>
/// Threat level enumeration.
/// Single responsibility: Threat severity values.
/// </summary>
public enum ThreatLevel
{
    /// <summary>
    /// No threat detected.
    /// </summary>
    None = 0,

    /// <summary>
    /// Low threat level.
    /// </summary>
    Low = 1,

    /// <summary>
    /// Medium threat level.
    /// </summary>
    Medium = 2,

    /// <summary>
    /// High threat level.
    /// </summary>
    High = 3,

    /// <summary>
    /// Critical threat - file must be quarantined.
    /// </summary>
    Critical = 4
}
