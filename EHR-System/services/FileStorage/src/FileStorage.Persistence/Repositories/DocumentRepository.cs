using Microsoft.EntityFrameworkCore;
using EHRPlatform.Services.FileStorage.Domain.Entities;

namespace EHRPlatform.Services.FileStorage.Persistence.Repositories;

/// <summary>
/// Repository for StoredDocument entity - specialized queries for document domain.
/// Includes document lookup, patient queries, and audit logging.
/// </summary>
public class DocumentRepository
{
    private readonly FileStorageContext _context;

    public DocumentRepository(FileStorageContext context)
    {
        _context = context;
    }

    public async Task<StoredDocument?> GetByIdAsync(Guid documentId, CancellationToken cancellationToken = default)
    {
        return await _context.StoredDocuments
            .Include(x => x.Versions)
            .Include(x => x.VirusScanResults)
            .Include(x => x.AccessHistory)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == documentId, cancellationToken);
    }

    public async Task<List<StoredDocument>> GetPatientDocumentsAsync(Guid patientId, CancellationToken cancellationToken = default)
    {
        return await _context.StoredDocuments
            .Where(x => x.PatientId == patientId && !x.IsMarkedForDeletion)
            .Include(x => x.VirusScanResults)
            .AsNoTracking()
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<StoredDocument?> GetByS3KeyAsync(string s3Key, CancellationToken cancellationToken = default)
    {
        return await _context.StoredDocuments
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.S3Key == s3Key, cancellationToken);
    }

    public async Task<StoredDocument?> GetByHashAsync(string fileHash, CancellationToken cancellationToken = default)
    {
        return await _context.StoredDocuments
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.FileHash == fileHash, cancellationToken);
    }

    public async Task<List<StoredDocument>> GetQuarantinedDocumentsAsync(CancellationToken cancellationToken = default)
    {
        return await _context.StoredDocuments
            .Where(x => x.Status == "Quarantined")
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<List<StoredDocument>> GetDocumentsMarkedForDeletionAsync(CancellationToken cancellationToken = default)
    {
        return await _context.StoredDocuments
            .Where(x => x.IsMarkedForDeletion && x.ScheduledDeletionDate <= DateTime.UtcNow)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<long> GetPatientStorageUsageAsync(Guid patientId, CancellationToken cancellationToken = default)
    {
        return await _context.StoredDocuments
            .Where(x => x.PatientId == patientId && !x.IsMarkedForDeletion)
            .SumAsync(x => x.FileSizeBytes, cancellationToken);
    }

    public async Task AddAsync(StoredDocument document, CancellationToken cancellationToken = default)
    {
        await _context.StoredDocuments.AddAsync(document, cancellationToken);
    }

    public void Update(StoredDocument document)
    {
        _context.StoredDocuments.Update(document);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}
