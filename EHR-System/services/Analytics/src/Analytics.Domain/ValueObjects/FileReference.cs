namespace EHRPlatform.Services.Analytics.Domain.ValueObjects;

/// <summary>
/// Value object representing a file reference with metadata
/// Used for report outputs, exports, and attachments
/// </summary>
public class FileReference : IEquatable<FileReference>
{
    /// <summary>
    /// Unique file identifier or path
    /// </summary>
    public string Path { get; }

    /// <summary>
    /// File name with extension
    /// </summary>
    public string FileName { get; }

    /// <summary>
    /// MIME type (e.g., application/pdf, text/csv)
    /// </summary>
    public string ContentType { get; }

    /// <summary>
    /// File size in bytes
    /// </summary>
    public long SizeBytes { get; }

    /// <summary>
    /// When file was created
    /// </summary>
    public DateTime CreatedAt { get; }

    /// <summary>
    /// Creates new FileReference
    /// </summary>
    public FileReference(string path, string fileName, string contentType, long sizeBytes, DateTime createdAt)
    {
        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentException("Path cannot be empty", nameof(path));
        if (string.IsNullOrWhiteSpace(fileName))
            throw new ArgumentException("FileName cannot be empty", nameof(fileName));
        if (string.IsNullOrWhiteSpace(contentType))
            throw new ArgumentException("ContentType cannot be empty", nameof(contentType));
        if (sizeBytes < 0)
            throw new ArgumentException("Size cannot be negative", nameof(sizeBytes));

        Path = path;
        FileName = fileName;
        ContentType = contentType;
        SizeBytes = sizeBytes;
        CreatedAt = createdAt;
    }

    /// <summary>
    /// Gets human-readable file size (e.g., "1.5 MB")
    /// </summary>
    public string GetFormattedSize()
    {
        string[] sizes = { "B", "KB", "MB", "GB", "TB" };
        double len = SizeBytes;
        int order = 0;
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len = len / 1024;
        }
        return $"{len:0.##} {sizes[order]}";
    }

    /// <summary>
    /// Gets file extension without dot
    /// </summary>
    public string GetExtension() => System.IO.Path.GetExtension(FileName).TrimStart('.');

    /// <summary>
    /// Checks if file is a common format
    /// </summary>
    public bool IsCsv() => ContentType == "text/csv" || GetExtension() == "csv";
    public bool IsPdf() => ContentType == "application/pdf" || GetExtension() == "pdf";
    public bool IsJson() => ContentType == "application/json" || GetExtension() == "json";
    public bool IsExcel() => ContentType.Contains("spreadsheet") || GetExtension() is "xlsx" or "xls";

    public bool Equals(FileReference? other)
    {
        if (other is null) return false;
        return Path == other.Path && FileName == other.FileName && ContentType == other.ContentType;
    }

    public override bool Equals(object? obj) => Equals(obj as FileReference);

    public override int GetHashCode() => HashCode.Combine(Path, FileName, ContentType);

    public override string ToString() => $"{FileName} ({GetFormattedSize()})";
}
