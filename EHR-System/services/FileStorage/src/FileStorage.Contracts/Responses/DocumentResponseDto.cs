namespace EHRPlatform.Services.FileStorage.Contracts.Responses;

/// <summary>
/// Document response DTO for API responses.
/// </summary>
public class DocumentResponseDto
{
    public Guid Id { get; set; }
    public Guid PatientId { get; set; }
    public Guid UploadedBy { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public string FileHash { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Classification { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsEncrypted { get; set; }
    public bool IsMarkedForDeletion { get; set; }
    public DateTime? ScheduledDeletionDate { get; set; }
    public List<VirusScanResultDto> VirusScanResults { get; set; } = new();
    public List<DocumentAccessDto> RecentAccess { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class VirusScanResultDto
{
    public Guid Id { get; set; }
    public string ScannerName { get; set; } = string.Empty;
    public string Result { get; set; } = string.Empty;
    public string? ThreatName { get; set; }
    public DateTime ScannedAt { get; set; }
}

public class DocumentAccessDto
{
    public Guid Id { get; set; }
    public Guid AccessedBy { get; set; }
    public string AccessType { get; set; } = string.Empty;
    public DateTime AccessedAt { get; set; }
}
