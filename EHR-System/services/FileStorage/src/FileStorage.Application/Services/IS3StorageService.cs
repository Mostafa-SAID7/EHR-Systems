namespace EHRPlatform.Services.FileStorage.Application.Services;

/// <summary>
/// Interface for S3 storage operations.
/// Handles file upload, download, and deletion from AWS S3.
/// </summary>
public interface IS3StorageService
{
    /// <summary>
    /// Uploads file to S3 bucket.
    /// </summary>
    Task<string> UploadFileAsync(string bucket, string key, Stream fileStream, string contentType, CancellationToken cancellationToken = default);

    /// <summary>
    /// Downloads file from S3 as stream.
    /// </summary>
    Task<Stream> DownloadFileAsync(string bucket, string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes file from S3 bucket.
    /// </summary>
    Task DeleteFileAsync(string bucket, string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates pre-signed URL for temporary download access.
    /// </summary>
    Task<string> GeneratePresignedUrlAsync(string bucket, string key, TimeSpan expiration, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if file exists in S3.
    /// </summary>
    Task<bool> FileExistsAsync(string bucket, string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets file metadata from S3.
    /// </summary>
    Task<S3FileMetadata> GetFileMetadataAsync(string bucket, string key, CancellationToken cancellationToken = default);
}

public class S3FileMetadata
{
    public string Key { get; set; } = string.Empty;
    public long ContentLength { get; set; }
    public string ContentType { get; set; } = string.Empty;
    public DateTime LastModified { get; set; }
    public string ETag { get; set; } = string.Empty;
}
