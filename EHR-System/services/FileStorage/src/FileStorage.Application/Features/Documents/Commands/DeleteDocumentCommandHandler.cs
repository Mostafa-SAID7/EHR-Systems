namespace EHRPlatform.Services.FileStorage.Application.Features.Documents.Commands;

using MediatR;
using EHRPlatform.Services.FileStorage.Domain.Entities;
using EHRPlatform.Services.FileStorage.Persistence;
using EHRPlatform.Services.FileStorage.Application.Services;
using Microsoft.Extensions.Logging;

/// <summary>
/// Handler for DeleteDocumentCommand - Deletes document from storage.
/// Either immediately or schedules deletion based on retention policy.
/// </summary>
public class DeleteDocumentCommandHandler : IRequestHandler<DeleteDocumentCommand, DeleteDocumentResponse>
{
    private readonly IFileStorageDbContext _context;
    private readonly IS3StorageService _s3StorageService;
    private readonly IDocumentRetentionService _retentionService;
    private readonly ILogger<DeleteDocumentCommandHandler> _logger;

    public DeleteDocumentCommandHandler(
        IFileStorageDbContext context,
        IS3StorageService s3StorageService,
        IDocumentRetentionService retentionService,
        ILogger<DeleteDocumentCommandHandler> logger)
    {
        _context = context;
        _s3StorageService = s3StorageService;
        _retentionService = retentionService;
        _logger = logger;
    }

    public async Task<DeleteDocumentResponse> Handle(DeleteDocumentCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Processing deletion for document {DocumentId}. Immediate: {IsImmediate}", 
            request.DocumentId, request.IsImmediate);

        var document = await _context.StoredDocuments.FindAsync(new object[] { request.DocumentId }, cancellationToken);
        if (document == null)
        {
            throw new InvalidOperationException($"Document {request.DocumentId} not found");
        }

        if (request.IsImmediate)
        {
            // Immediate deletion
            await _s3StorageService.DeleteFileAsync(document.S3Bucket, document.S3Key, cancellationToken);
            _context.StoredDocuments.Remove(document);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Document {DocumentId} immediately deleted from storage", request.DocumentId);

            return new DeleteDocumentResponse
            {
                DocumentId = document.Id,
                DeleteScheduled = false,
                Message = "Document immediately deleted"
            };
        }
        else
        {
            // Scheduled deletion (soft delete)
            var scheduledDate = DateTime.UtcNow.AddDays(7); // 7-day grace period
            document.MarkForDeletion(scheduledDate);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Document {DocumentId} scheduled for deletion on {ScheduledDate}", 
                request.DocumentId, scheduledDate);

            return new DeleteDocumentResponse
            {
                DocumentId = document.Id,
                DeleteScheduled = true,
                ScheduledDeletionDate = scheduledDate,
                Message = "Document scheduled for deletion"
            };
        }
    }
}
