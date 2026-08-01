using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace EHRPlatform.Common.FileStorage;

/// <summary>
/// Interface for cloud storage (S3, Azure Blob, etc).
/// Single responsibility: Manage files in cloud storage.
/// </summary>
public interface ICloudStorage
{
    /// <summary>
    /// Upload file to cloud storage.
    /// </summary>
    Task<CloudFileReference> UploadAsync(Stream fileStream, string fileName, string contentType, Dictionary<string, string>? metadata = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Download file from cloud storage.
    /// </summary>
    Task<Stream> DownloadAsync(CloudFileReference reference, CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete file from cloud storage.
    /// </summary>
    Task<bool> DeleteAsync(CloudFileReference reference, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get public URL (with optional expiration).
    /// </summary>
    Task<string> GetPublicUrlAsync(CloudFileReference reference, TimeSpan? expiresIn = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get cloud file properties.
    /// </summary>
    Task<CloudFileProperties> GetPropertiesAsync(CloudFileReference reference, CancellationToken cancellationToken = default);

    /// <summary>
    /// Copy file within cloud storage.
    /// </summary>
    Task<CloudFileReference> CopyAsync(CloudFileReference source, string destinationPath, CancellationToken cancellationToken = default);

    /// <summary>
    /// List files in bucket/container.
    /// </summary>
    Task<IReadOnlyList<CloudFileReference>> ListAsync(string path, CancellationToken cancellationToken = default);
}

/// <summary>
/// Cloud file reference.
/// Single responsibility: Reference to cloud-stored file.
/// </summary>
public class CloudFileReference
{
    /// <summary>
    /// Bucket/container name.
    /// </summary>
    public string Bucket { get; set; } = null!;

    /// <summary>
    /// File key/path in bucket.
    /// </summary>
    public string Key { get; set; } = null!;

    /// <summary>
    /// Cloud provider (S3, AzureBlob, etc).
    /// </summary>
    public string Provider { get; set; } = null!;
}

/// <summary>
/// Cloud file properties.
/// Single responsibility: Cloud file metadata.
/// </summary>
public class CloudFileProperties
{
    /// <summary>
    /// File size in bytes.
    /// </summary>
    public long SizeBytes { get; set; }

    /// <summary>
    /// Content type.
    /// </summary>
    public string ContentType { get; set; } = null!;

    /// <summary>
    /// Last modified time.
    /// </summary>
    public DateTime LastModified { get; set; }

    /// <summary>
    /// Entity tag (ETag) for version/cache.
    /// </summary>
    public string? ETag { get; set; }

    /// <summary>
    /// File metadata/tags.
    /// </summary>
    public Dictionary<string, string>? Metadata { get; set; }
}
