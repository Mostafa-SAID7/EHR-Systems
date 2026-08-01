using System;

namespace EHRPlatform.Common.FileStorage;

/// <summary>
/// File metadata.
/// Single responsibility: File information data.
/// </summary>
public class FileMetadata
{
    /// <summary>
    /// File ID.
    /// </summary>
    public string Id { get; set; } = null!;

    /// <summary>
    /// Original file name.
    /// </summary>
    public string FileName { get; set; } = null!;

    /// <summary>
    /// File size in bytes.
    /// </summary>
    public long SizeBytes { get; set; }

    /// <summary>
    /// Content type/MIME type.
    /// </summary>
    public string ContentType { get; set; } = null!;

    /// <summary>
    /// Upload time.
    /// </summary>
    public DateTime UploadedAt { get; set; }

    /// <summary>
    /// File hash (for integrity).
    /// </summary>
    public string? Hash { get; set; }
}
