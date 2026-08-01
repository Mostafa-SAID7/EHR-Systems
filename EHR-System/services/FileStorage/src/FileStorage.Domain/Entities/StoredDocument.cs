namespace EHRPlatform.Services.FileStorage.Domain.Entities;

/// <summary>
/// StoredDocument aggregate root - File metadata and storage information.
/// Tracks document location, hash, encryption, and retention policies.
/// HIPAA Compliant: Supports encryption, retention, and audit logging.
/// </summary>
public class StoredDocument
{
    public Guid Id { get; set; }
    public Guid PatientId { get; set; }
    public Guid UploadedBy { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public string FileHash { get; set; } = string.Empty; // SHA-256 for integrity
    public string S3Key { get; set; } = string.Empty; // S3 bucket key (path)
    public string S3Bucket { get; set; } = string.Empty; // AWS S3 bucket name
    public string? EncryptionKeyId { get; set; } // For customer-managed keys
    public bool IsEncrypted { get; set; } // AES-256 encryption
    public string Status { get; set; } = "Uploaded"; // Uploaded, Scanned, Clean, Quarantined, Archived, Deleted
    public string Classification { get; set; } = "PHI"; // PHI, Public, Confidential
    public string Category { get; set; } = "Other"; // LabResult, Prescription, Imaging, Note, etc.
    public string? Description { get; set; }
    public Guid? RetentionPolicyId { get; set; }
    public bool IsMarkedForDeletion { get; set; }
    public DateTime? ScheduledDeletionDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public ICollection<DocumentVersion> Versions { get; } = new List<DocumentVersion>();
    public ICollection<VirusScanResult> VirusScanResults { get; } = new List<VirusScanResult>();
    public ICollection<DocumentAccess> AccessHistory { get; } = new List<DocumentAccess>();

    private readonly List<object> _domainEvents = new();

    public void RecordUpload(Guid patientId, Guid uploadedBy, string fileName, string contentType, long fileSizeBytes, string fileHash)
    {
        PatientId = patientId;
        UploadedBy = uploadedBy;
        FileName = fileName;
        ContentType = contentType;
        FileSizeBytes = fileSizeBytes;
        FileHash = fileHash;
        Status = "Uploaded";
        CreatedAt = DateTime.UtcNow;
        RaiseEvent(new DocumentUploadedEvent(Id, PatientId, FileName, FileSizeBytes));
    }

    public void MarkAsScanned()
    {
        Status = "Scanned";
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkAsClean()
    {
        Status = "Clean";
        UpdatedAt = DateTime.UtcNow;
        RaiseEvent(new VirusScanCompletedEvent(Id, PatientId, "Clean", null));
    }

    public void MarkAsQuarantined(string threatDetails)
    {
        Status = "Quarantined";
        UpdatedAt = DateTime.UtcNow;
        RaiseEvent(new VirusScanCompletedEvent(Id, PatientId, "Quarantined", threatDetails));
    }

    public void MarkAsArchived()
    {
        Status = "Archived";
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkForDeletion(DateTime? scheduledDeletionDate = null)
    {
        IsMarkedForDeletion = true;
        ScheduledDeletionDate = scheduledDeletionDate ?? DateTime.UtcNow.AddDays(7);
        UpdatedAt = DateTime.UtcNow;
    }

    public void UnmarkForDeletion()
    {
        IsMarkedForDeletion = false;
        ScheduledDeletionDate = null;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetEncryption(bool encrypted, string? keyId = null)
    {
        IsEncrypted = encrypted;
        EncryptionKeyId = keyId;
        UpdatedAt = DateTime.UtcNow;
    }

    public void RaiseEvent(object @event) => _domainEvents.Add(@event);
    public IReadOnlyList<object> GetDomainEvents() => _domainEvents.AsReadOnly();
    public void ClearDomainEvents() => _domainEvents.Clear();
}

/// <summary>
/// Document version tracking for audit and recovery.
/// </summary>
public class DocumentVersion
{
    public Guid Id { get; set; }
    public Guid DocumentId { get; set; }
    public int VersionNumber { get; set; }
    public string S3Key { get; set; } = string.Empty;
    public string FileHash { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public Guid UploadedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public StoredDocument Document { get; set; } = null!;
}

/// <summary>
/// Virus scan results for compliance and threat tracking.
/// </summary>
public class VirusScanResult
{
    public Guid Id { get; set; }
    public Guid DocumentId { get; set; }
    public string ScannerName { get; set; } = string.Empty; // ClamAV, etc.
    public string Result { get; set; } = "CLEAN"; // CLEAN, INFECTED, ERROR
    public string? ThreatName { get; set; }
    public DateTime ScannedAt { get; set; }
    public string? ScanDetails { get; set; }
    public StoredDocument Document { get; set; } = null!;
}

/// <summary>
/// Document access audit log for HIPAA compliance.
/// </summary>
public class DocumentAccess
{
    public Guid Id { get; set; }
    public Guid DocumentId { get; set; }
    public Guid AccessedBy { get; set; }
    public string AccessType { get; set; } = string.Empty; // VIEW, DOWNLOAD, DELETE
    public string? Reason { get; set; }
    public DateTime AccessedAt { get; set; }
    public string IpAddress { get; set; } = string.Empty;
    public StoredDocument Document { get; set; } = null!;
}

// Domain Events
public record DocumentUploadedEvent(Guid DocumentId, Guid PatientId, string FileName, long FileSizeBytes)
{
    public DateTime CreatedAt { get; } = DateTime.UtcNow;
}

public record VirusScanCompletedEvent(Guid DocumentId, Guid PatientId, string Result, string? ThreatName)
{
    public DateTime CreatedAt { get; } = DateTime.UtcNow;
}

public record DocumentDeletedEvent(Guid DocumentId, Guid PatientId, string Reason)
{
    public DateTime CreatedAt { get; } = DateTime.UtcNow;
}
