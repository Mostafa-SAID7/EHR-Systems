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
