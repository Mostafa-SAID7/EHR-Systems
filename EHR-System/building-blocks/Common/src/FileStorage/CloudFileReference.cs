namespace EHRPlatform.Common.FileStorage;

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
