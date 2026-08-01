using Microsoft.Extensions.Logging;
using EHRPlatform.Services.FileStorage.Contracts.Responses;
using EHRPlatform.Services.FileStorage.Domain.Entities;

namespace EHRPlatform.Services.FileStorage.Application.Features.Documents.Mappers;

/// <summary>
/// Maps StoredDocument domain model to DTOs for API responses.
/// </summary>
public class DocumentMapper
{
    private readonly ILogger<DocumentMapper> _logger;

    public DocumentMapper(ILogger<DocumentMapper> logger)
    {
        _logger = logger;
    }

    public DocumentResponseDto MapToResponseDto(StoredDocument document)
    {
        return new DocumentResponseDto
        {
            Id = document.Id,
            PatientId = document.PatientId,
            UploadedBy = document.UploadedBy,
            FileName = document.FileName,
            ContentType = document.ContentType,
            FileSizeBytes = document.FileSizeBytes,
            FileHash = document.FileHash,
            Status = document.Status,
            Classification = document.Classification,
            Category = document.Category,
            Description = document.Description,
            IsEncrypted = document.IsEncrypted,
            IsMarkedForDeletion = document.IsMarkedForDeletion,
            ScheduledDeletionDate = document.ScheduledDeletionDate,
            VirusScanResults = document.VirusScanResults
                .OrderByDescending(x => x.ScannedAt)
                .Select(MapVirusScanResult)
                .ToList(),
            RecentAccess = document.AccessHistory
                .OrderByDescending(x => x.AccessedAt)
                .Take(10)
                .Select(MapDocumentAccess)
                .ToList(),
            CreatedAt = document.CreatedAt,
            UpdatedAt = document.UpdatedAt
        };
    }

    private static VirusScanResultDto MapVirusScanResult(VirusScanResult scan)
    {
        return new VirusScanResultDto
        {
            Id = scan.Id,
            ScannerName = scan.ScannerName,
            Result = scan.Result,
            ThreatName = scan.ThreatName,
            ScannedAt = scan.ScannedAt
        };
    }

    private static DocumentAccessDto MapDocumentAccess(DocumentAccess access)
    {
        return new DocumentAccessDto
        {
            Id = access.Id,
            AccessedBy = access.AccessedBy,
            AccessType = access.AccessType,
            AccessedAt = access.AccessedAt
        };
    }

    public List<DocumentResponseDto> MapToResponseDtoList(IEnumerable<StoredDocument> documents)
    {
        return documents.Select(MapToResponseDto).ToList();
    }
}
