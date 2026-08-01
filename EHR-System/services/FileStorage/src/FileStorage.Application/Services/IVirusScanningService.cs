namespace EHRPlatform.Services.FileStorage.Application.Services;

/// <summary>
/// Interface for virus scanning service.
/// Abstracts ClamAV integration and scanning logic.
/// </summary>
public interface IVirusScanningService
{
    /// <summary>
    /// Initiates async virus scan for a document in S3.
    /// </summary>
    Task<string> InitiateScanAsync(Guid documentId, string s3Key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks scan status by job ID.
    /// </summary>
    Task<VirusScanStatus> GetScanStatusAsync(string scanJobId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Downloads file from S3, scans locally, uploads result.
    /// </summary>
    Task<VirusScanResult> ScanFileAsync(string s3Bucket, string s3Key, CancellationToken cancellationToken = default);
}

public class VirusScanStatus
{
    public string JobId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty; // Pending, Scanning, Complete, Error
    public string? Result { get; set; } // CLEAN, INFECTED, ERROR
    public string? ThreatName { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}

public class VirusScanResult
{
    public string Status { get; set; } = string.Empty; // CLEAN, INFECTED, ERROR
    public string? ThreatName { get; set; }
    public string? Details { get; set; }
    public DateTime ScannedAt { get; set; }
}
