namespace EHRPlatform.Services.FileStorage.Application.Services;

/// <summary>
/// Interface for document retention policy management.
/// Handles retention schedules, expiration, and cleanup.
/// </summary>
public interface IDocumentRetentionService
{
    /// <summary>
    /// Applies retention policy to a document.
    /// </summary>
    Task ApplyRetentionPolicyAsync(Guid documentId, Guid policyId, int retentionDays, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets retention expiration date for a document.
    /// </summary>
    Task<DateTime?> GetExpirationDateAsync(Guid documentId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Finds all documents eligible for deletion based on retention policies.
    /// </summary>
    Task<IEnumerable<Guid>> FindDocumentsForDeletionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes retention cleanup - deletes expired documents.
    /// Returns count of deleted documents.
    /// </summary>
    Task<int> ExecuteRetentionCleanupAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Extends retention period for a document.
    /// </summary>
    Task ExtendRetentionAsync(Guid documentId, int additionalDays, CancellationToken cancellationToken = default);
}

public class RetentionPolicy
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string DocumentCategory { get; set; } = string.Empty; // LabResult, Prescription, etc.
    public int DefaultRetentionDays { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}
