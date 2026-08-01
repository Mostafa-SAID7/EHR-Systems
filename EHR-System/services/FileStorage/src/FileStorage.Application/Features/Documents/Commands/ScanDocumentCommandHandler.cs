namespace EHRPlatform.Services.FileStorage.Application.Features.Documents.Commands;

using MediatR;
using EHRPlatform.Services.FileStorage.Domain.Entities;
using EHRPlatform.Services.FileStorage.Persistence;
using EHRPlatform.Services.FileStorage.Application.Services;
using Microsoft.Extensions.Logging;

/// <summary>
/// Handler for ScanDocumentCommand - Initiates virus scanning.
/// Queues document for async scanning via virus scanning service.
/// </summary>
public class ScanDocumentCommandHandler : IRequestHandler<ScanDocumentCommand, ScanDocumentResponse>
{
    private readonly IFileStorageDbContext _context;
    private readonly IVirusScanningService _virusScanningService;
    private readonly ILogger<ScanDocumentCommandHandler> _logger;

    public ScanDocumentCommandHandler(
        IFileStorageDbContext context,
        IVirusScanningService virusScanningService,
        ILogger<ScanDocumentCommandHandler> logger)
    {
        _context = context;
        _virusScanningService = virusScanningService;
        _logger = logger;
    }

    public async Task<ScanDocumentResponse> Handle(ScanDocumentCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Initiating virus scan for document {DocumentId}", request.DocumentId);

        var document = await _context.StoredDocuments.FindAsync(new object[] { request.DocumentId }, cancellationToken);
        if (document == null)
        {
            throw new InvalidOperationException($"Document {request.DocumentId} not found");
        }

        // Mark as scanned to prevent multiple concurrent scans
        document.MarkAsScanned();

        // Queue document for virus scanning
        var scanJobId = await _virusScanningService.InitiateScanAsync(
            request.DocumentId,
            request.S3Key,
            cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Virus scan initiated for document {DocumentId} with job {JobId}", 
            request.DocumentId, scanJobId);

        return new ScanDocumentResponse
        {
            DocumentId = document.Id,
            ScanInitiated = true,
            ScanJobId = scanJobId,
            ScanStartedAt = DateTime.UtcNow
        };
    }
}
