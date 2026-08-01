using MediatR;
using Microsoft.Extensions.Logging;
using EHRPlatform.Services.FileStorage.Application.Features.Documents.Commands;
using EHRPlatform.Services.FileStorage.Application.Services;
using EHRPlatform.Services.FileStorage.Domain.Entities;
using EHRPlatform.Services.FileStorage.Persistence;

namespace EHRPlatform.Services.FileStorage.Application.Features.Documents.Handlers;

/// <summary>
/// Handler for UploadDocumentCommand.
/// Uploads file to S3, stores metadata, triggers virus scan.
/// Publishes DocumentUploadedEvent for subscriber services.
/// </summary>
public class UploadDocumentCommandHandler : IRequestHandler<UploadDocumentCommand, UploadDocumentResponse>
{
    private readonly IFileStorageDbContext _context;
    private readonly IS3StorageService _s3Service;
    private readonly IVirusScanningService _virusScanningService;
    private readonly ILogger<UploadDocumentCommandHandler> _logger;

    public UploadDocumentCommandHandler(
        IFileStorageDbContext context,
        IS3StorageService s3Service,
        IVirusScanningService virusScanningService,
        ILogger<UploadDocumentCommandHandler> logger)
    {
        _context = context;
        _s3Service = s3Service;
        _virusScanningService = virusScanningService;
        _logger = logger;
    }

    public async Task<UploadDocumentResponse> Handle(
        UploadDocumentCommand command,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation(
                "Uploading document for patient {PatientId}: {FileName}",
                command.PatientId, command.FileName);

            // Validate input
            if (command.PatientId == Guid.Empty)
                throw new ArgumentException("PatientId cannot be empty");

            if (string.IsNullOrEmpty(command.FileName))
                throw new ArgumentException("FileName cannot be empty");

            if (command.FileContent == null || command.FileContent.Length == 0)
                throw new ArgumentException("File content cannot be empty");

            // Validate file size (max 100MB)
            const long maxFileSize = 100L * 1024L * 1024L;
            if (command.FileContent.Length > maxFileSize)
                throw new InvalidOperationException($"File size exceeds maximum allowed size of 100MB");

            // Validate file type
            var allowedExtensions = new[] { ".pdf", ".jpg", ".jpeg", ".png", ".docx", ".xlsx", ".txt", ".dicom" };
            var fileExtension = Path.GetExtension(command.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(fileExtension))
                throw new InvalidOperationException($"File type {fileExtension} is not allowed");

            // Generate S3 key
            var s3Key = GenerateS3Key(command.PatientId, command.FileName);

            // Upload to S3
            var uploadResult = await _s3Service.UploadAsync(
                s3Key,
                command.FileContent,
                command.ContentType,
                cancellationToken);

            if (!uploadResult.Success)
                throw new InvalidOperationException($"S3 upload failed: {uploadResult.ErrorMessage}");

            // Create document entity
            var document = new StoredDocument
            {
                Id = Guid.NewGuid(),
                PatientId = command.PatientId,
                ProviderId = command.ProviderId,
                FileName = command.FileName,
                ContentType = command.ContentType,
                S3Key = s3Key,
                FileSizeBytes = command.FileContent.Length,
                DocumentType = command.DocumentType,
                Description = command.Description,
                UploadStatus = "Uploaded",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsDeleted = false
            };

            await _context.StoredDocuments.AddAsync(document, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            // Initiate virus scan
            string? scanJobId = null;
            try
            {
                scanJobId = await _virusScanningService.InitiateScanAsync(
                    document.Id,
                    s3Key,
                    cancellationToken);

                document.UploadStatus = "ScanPending";
                document.ScanJobId = scanJobId;
                await _context.SaveChangesAsync(cancellationToken);

                _logger.LogInformation(
                    "Virus scan initiated for document {DocumentId} with job {JobId}",
                    document.Id, scanJobId);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(
                    ex,
                    "Failed to initiate virus scan for document {DocumentId}. Document still uploaded.",
                    document.Id);
                // Don't fail the upload if scan fails to initiate
            }

            _logger.LogInformation(
                "Document uploaded successfully: {DocumentId}, Size: {FileSizeBytes} bytes",
                document.Id, document.FileSizeBytes);

            return new UploadDocumentResponse
            {
                DocumentId = document.Id,
                FileName = document.FileName,
                S3Key = s3Key,
                FileSizeBytes = document.FileSizeBytes,
                UploadStatus = document.UploadStatus,
                UploadedAt = document.CreatedAt,
                ScanJobId = scanJobId
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error uploading document for patient {PatientId}",
                command.PatientId);
            throw;
        }
    }

    private static string GenerateS3Key(Guid patientId, string fileName)
    {
        // S3 key format: ehr/documents/{patientId}/{date}/{fileName}
        var date = DateTime.UtcNow.ToString("yyyy/MM/dd");
        var safeFileName = Path.GetFileName(fileName).Replace(" ", "_");
        var uniqueSuffix = Guid.NewGuid().ToString("N").Substring(0, 8);
        
        return $"ehr/documents/{patientId}/{date}/{uniqueSuffix}_{safeFileName}";
    }
}

/// <summary>
/// S3 Storage Service Interface
/// </summary>
public interface IS3StorageService
{
    Task<S3UploadResult> UploadAsync(string key, byte[] content, string contentType, CancellationToken cancellationToken);
}

/// <summary>
/// S3 Upload Result
/// </summary>
public class S3UploadResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public string? S3Url { get; set; }
}

/// <summary>
/// Domain entity for stored documents
/// </summary>
public class StoredDocument
{
    public Guid Id { get; set; }
    public Guid PatientId { get; set; }
    public Guid ProviderId { get; set; }
    public string FileName { get; set; }
    public string ContentType { get; set; }
    public string S3Key { get; set; }
    public long FileSizeBytes { get; set; }
    public string DocumentType { get; set; }
    public string? Description { get; set; }
    public string UploadStatus { get; set; } // "Uploaded", "ScanPending", "Scanned", "Clean", "Infected"
    public string? ScanJobId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public bool IsDeleted { get; set; }

    public void MarkAsScanned() => UploadStatus = "Scanned";
    public void MarkAsClean() => UploadStatus = "Clean";
    public void MarkAsInfected() => UploadStatus = "Infected";
}
