using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace EHRPlatform.Common.FileStorage;

/// <summary>
/// Interface for file storage (local or cloud).
/// Single responsibility: Store and retrieve files.
/// </summary>
public interface IFileStorage
{
    /// <summary>
    /// Upload file.
    /// </summary>
    Task<string> UploadAsync(Stream fileStream, string fileName, string contentType, CancellationToken cancellationToken = default);

    /// <summary>
    /// Download file.
    /// </summary>
    Task<Stream> DownloadAsync(string fileId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete file.
    /// </summary>
    Task<bool> DeleteAsync(string fileId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get file metadata.
    /// </summary>
    Task<FileMetadata?> GetFileMetadataAsync(string fileId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if file exists.
    /// </summary>
    Task<bool> ExistsAsync(string fileId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get file URL (if applicable).
    /// </summary>
    Task<string?> GetFileUrlAsync(string fileId, TimeSpan? expiresIn = null, CancellationToken cancellationToken = default);
}
