#nullable enable

using System.Linq.Expressions;

namespace EHRPlatform.Common.Data.Abstractions;

/// <summary>
/// Generic repository interface for MongoDB documents.
/// Mirrors <see cref="IRepository{TEntity}"/> patterns so services can swap
/// between EF Core (relational) and MongoDB (document) with minimal friction.
///
/// Use for: clinical documents, progress notes, scanned PDF metadata,
/// high-volume audit logs, device-generated vitals streams.
/// </summary>
/// <typeparam name="TDocument">MongoDB document type derived from <see cref="MongoBaseDocument"/>.</typeparam>
public interface IMongoRepository<TDocument> where TDocument : MongoBaseDocument
{
    /// <summary>Get a document by its string Id.</summary>
    Task<TDocument?> GetByIdAsync(string id, CancellationToken cancellationToken = default);

    /// <summary>Get a document by the linked EntityId (GUID from relational DB).</summary>
    Task<TDocument?> GetByEntityIdAsync(Guid entityId, CancellationToken cancellationToken = default);

    /// <summary>Get all non-deleted documents.</summary>
    Task<IEnumerable<TDocument>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>Get a paginated list of non-deleted documents.</summary>
    Task<(IEnumerable<TDocument> items, long totalCount)> GetPagedAsync(
        int pageNumber,
        int pageSize,
        Expression<Func<TDocument, bool>>? filter = null,
        CancellationToken cancellationToken = default);

    /// <summary>Find documents matching a filter expression.</summary>
    Task<IEnumerable<TDocument>> FindAsync(
        Expression<Func<TDocument, bool>> filter,
        CancellationToken cancellationToken = default);

    /// <summary>Find a single document or null.</summary>
    Task<TDocument?> FindOneAsync(
        Expression<Func<TDocument, bool>> filter,
        CancellationToken cancellationToken = default);

    /// <summary>Insert a new document.</summary>
    Task InsertAsync(TDocument document, CancellationToken cancellationToken = default);

    /// <summary>Insert multiple documents in one batch.</summary>
    Task InsertManyAsync(IEnumerable<TDocument> documents, CancellationToken cancellationToken = default);

    /// <summary>Replace an existing document by Id.</summary>
    Task ReplaceAsync(TDocument document, CancellationToken cancellationToken = default);

    /// <summary>Soft-delete a document (sets DeletedAt).</summary>
    Task DeleteAsync(string id, CancellationToken cancellationToken = default);

    /// <summary>Hard-delete a document from the collection.</summary>
    Task HardDeleteAsync(string id, CancellationToken cancellationToken = default);

    /// <summary>Count documents matching a filter.</summary>
    Task<long> CountAsync(
        Expression<Func<TDocument, bool>>? filter = null,
        CancellationToken cancellationToken = default);

    /// <summary>Check whether any document matches the filter.</summary>
    Task<bool> AnyAsync(
        Expression<Func<TDocument, bool>> filter,
        CancellationToken cancellationToken = default);

    /// <summary>Restore a soft-deleted document (clears DeletedAt).</summary>
    Task RestoreAsync(string id, CancellationToken cancellationToken = default);
}

