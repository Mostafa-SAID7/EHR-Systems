namespace EHRPlatform.Services.FileStorage.Infrastructure.Services;

using EHRPlatform.Services.FileStorage.Application.Services;
using EHRPlatform.Services.FileStorage.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

/// <summary>
/// Document retention policy enforcement service.
/// Manages retention schedules and automatic cleanup.
/// </summary>
public class DocumentRetentionService : IDocumentRetentionService
{
    private readonly IFileStorageDbContext _context;
    private readonly IS3StorageService _s3StorageService;
    private readonly ILogger<DocumentRetentionService> _logger;

    public DocumentRetentionService(
        IFileStorageDbContext context,
        IS3StorageService s3StorageService,
        ILogger<DocumentRetentionService> logger)
    {
        _context = context;
        _s3StorageService = s3StorageService;
        _logger = logger;
    }

    public async Task ApplyRetentionPolicyAsync(Guid documentId, Guid policyId, int retentionDays, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Applying retention policy {PolicyId} to document {DocumentId} for {RetentionDays} days", 
            policyId, documentId, retentionDays);

        var document = await _context.StoredDocuments.FindAsync(new object[] { documentId }, cancellationToken);
        if (document == null)
        {
            throw new InvalidOperationException($"Document {documentId} not found");
        }

        var expirationDate = DateTime.UtcNow.AddDays(retentionDays);
        document.RetentionPolicyId = policyId;
        document.ScheduledDeletionDate = expirationDate;
        document.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Retention policy applied. Expiration: {ExpirationDate}", expirationDate);
    }

    public async Task<DateTime?> GetExpirationDateAsync(Guid documentId, CancellationToken cancellationToken = default)
    {
        var document = await _context.StoredDocuments
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.Id == documentId, cancellationToken);

        return document?.ScheduledDeletionDate;
    }

    public async Task<IEnumerable<Guid>> FindDocumentsForDeletionAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Finding documents eligible for deletion");

        var documentsToDelete = await _context.StoredDocuments
            .AsNoTracking()
            .Where(d => d.IsMarkedForDeletion && d.ScheduledDeletionDate.HasValue && d.ScheduledDeletionDate <= DateTime.UtcNow)
            .Select(d => d.Id)
            .ToListAsync(cancellationToken);

        _logger.LogInformation("Found {Count} documents eligible for deletion", documentsToDelete.Count);
        return documentsToDelete;
    }

    public async Task<int> ExecuteRetentionCleanupAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting retention cleanup process");

        var documentsToDelete = await FindDocumentsForDeletionAsync(cancellationToken);
        var deletedCount = 0;

        foreach (var documentId in documentsToDelete)
        {
            try
            {
                var document = await _context.StoredDocuments.FindAsync(new object[] { documentId }, cancellationToken);
                if (document != null)
                {
                    // Delete from S3
                    await _s3StorageService.DeleteFileAsync(document.S3Bucket, document.S3Key, cancellationToken);

                    // Delete from database
                    _context.StoredDocuments.Remove(document);
                    deletedCount++;

                    _logger.LogInformation("Deleted document {DocumentId}", documentId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting document {DocumentId}", documentId);
            }
        }

        if (deletedCount > 0)
        {
            await _context.SaveChangesAsync(cancellationToken);
        }

        _logger.LogInformation("Retention cleanup completed. Deleted {Count} documents", deletedCount);
        return deletedCount;
    }

    public async Task ExtendRetentionAsync(Guid documentId, int additionalDays, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Extending retention for document {DocumentId} by {AdditionalDays} days", 
            documentId, additionalDays);

        var document = await _context.StoredDocuments.FindAsync(new object[] { documentId }, cancellationToken);
        if (document == null)
        {
            throw new InvalidOperationException($"Document {documentId} not found");
        }

        if (document.ScheduledDeletionDate.HasValue)
        {
            document.ScheduledDeletionDate = document.ScheduledDeletionDate.Value.AddDays(additionalDays);
        }
        else
        {
            document.ScheduledDeletionDate = DateTime.UtcNow.AddDays(additionalDays);
        }

        document.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Retention extended. New expiration: {ExpirationDate}", document.ScheduledDeletionDate);
    }
}
