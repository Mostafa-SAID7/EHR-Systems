using MediatR;

namespace EHRPlatform.Services.FileStorage.Application.Features.Documents.Commands;

/// <summary>
/// Command to upload a document to file storage.
/// Handles file validation, S3 upload, metadata storage, and virus scan trigger.
/// </summary>
public class UploadDocumentCommand : IRequest<UploadDocumentResponse>
{
    public Guid PatientId { get; set; }
    public Guid ProviderId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public byte[] FileContent { get; set; } = Array.Empty<byte>();
    public string DocumentType { get; set; } = string.Empty; // "LabResult", "Prescription", "ImageScan", "Note", etc
    public string? Description { get; set; }
}

/// <summary>
/// Response after successful document upload
/// </summary>
public class UploadDocumentResponse
{
    public Guid DocumentId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string S3Key { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public string UploadStatus { get; set; } = "Uploaded"; // "Uploaded", "ScanPending", "Scanned"
    public DateTime UploadedAt { get; set; }
    public string? ScanJobId { get; set; }
}
