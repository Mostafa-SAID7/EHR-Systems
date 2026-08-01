using System;
using System.Collections.Generic;

namespace EHRPlatform.Common.FileStorage;

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
