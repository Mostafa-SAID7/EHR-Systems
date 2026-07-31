#nullable enable

using System.Linq.Expressions;
using EHRPlatform.Common.Data.Abstractions;
using EHRPlatform.Common.Data.Models;
using EHRPlatform.Common.Data.Filters;
using EHRPlatform.Common.Shared.Utilities.Guards;
using EHRPlatform.Common.Shared.Utilities.Helpers;
using MongoDB.Driver;

namespace EHRPlatform.Common.Data.Implementations;

/// <summary>
/// Generic MongoDB repository.
/// Uses the official MongoDB .NET driver with strongly-typed LINQ-style filters.
///
/// Collection naming convention: lower-kebab plural of the document type name.
/// e.g. ClinicalNote → "clinical-notes".
///
/// All read operations automatically exclude soft-deleted documents unless the caller
/// explicitly supplies a filter that includes them.
/// </summary>
/// <typeparam name="TDocument">MongoDB document type derived from <see cref="MongoBaseDocument"/>.</typeparam>
public class MongoRepository<TDocument> : IMongoRepository<TDocument>
    where TDocument : MongoBaseDocument
{
    protected readonly IMongoCollection<TDocument> _collection;

    public MongoRepository(IMongoDatabase database)
    {
        ArgumentNullException.ThrowIfNull(database);
        var collectionName = GetCollectionName();
        _collection = database.GetCollection<TDocument>(collectionName);
    }

    // ─── Reads ───────────────────────────────────────────────────────────────

    public async Task<TDocument?> GetByIdAsync(
        string id,
        CancellationToken cancellationToken = default)
    {
        ArgumentGuard.NotNullOrEmpty(id, nameof(id));
        var filter = NotDeletedFilter.GetById<TDocument>(id);
        return await _collection.Find(filter).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<TDocument?> GetByEntityIdAsync(
        Guid entityId,
        CancellationToken cancellationToken = default)
    {
        var filter = NotDeletedFilter.GetByEntityId<TDocument>(entityId);
        return await _collection.Find(filter).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IEnumerable<TDocument>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await _collection.Find(NotDeletedFilter.Get<TDocument>()).ToListAsync(cancellationToken);
    }

    public async Task<(IEnumerable<TDocument> items, long totalCount)> GetPagedAsync(
        int pageNumber,
        int pageSize,
        Expression<Func<TDocument, bool>>? filter = null,
        CancellationToken cancellationToken = default)
    {
        if (pageNumber < 1) throw new ArgumentException("Page number must be >= 1", nameof(pageNumber));
        if (pageSize < 1 || pageSize > 100) throw new ArgumentException("Page size must be 1–100", nameof(pageSize));

        var combinedFilter = NotDeletedFilter.CombineWithExpression(filter);

        var totalCount = await _collection.CountDocumentsAsync(combinedFilter, null, cancellationToken);
        var items = await _collection.Find(combinedFilter)
            .Skip((pageNumber - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<IEnumerable<TDocument>> FindAsync(
        Expression<Func<TDocument, bool>> filter,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(filter);
        var combinedFilter = NotDeletedFilter.CombineWithExpression(filter);
        return await _collection.Find(combinedFilter).ToListAsync(cancellationToken);
    }

    public async Task<TDocument?> FindOneAsync(
        Expression<Func<TDocument, bool>> filter,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(filter);
        var combinedFilter = NotDeletedFilter.CombineWithExpression(filter);
        return await _collection.Find(combinedFilter).FirstOrDefaultAsync(cancellationToken);
    }

    // ─── Writes ──────────────────────────────────────────────────────────────

    public async Task InsertAsync(TDocument document, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(document);
        document.CreatedAt = DateTimeHelper.UtcNow;
        document.UpdatedAt = DateTimeHelper.UtcNow;
        await _collection.InsertOneAsync(document, null, cancellationToken);
    }

    public async Task InsertManyAsync(
        IEnumerable<TDocument> documents,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(documents);
        var list = documents.ToList();
        if (list.Count == 0) return;
        var now = DateTimeHelper.UtcNow;
        foreach (var doc in list) { doc.CreatedAt = now; doc.UpdatedAt = now; }
        await _collection.InsertManyAsync(list, null, cancellationToken);
    }

    public async Task ReplaceAsync(TDocument document, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(document);
        document.UpdatedAt = DateTimeHelper.UtcNow;
        var filter = Builders<TDocument>.Filter.Eq(d => d.Id, document.Id);
        await _collection.ReplaceOneAsync(filter, document, new ReplaceOptions { IsUpsert = false }, cancellationToken);
    }

    public async Task DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        ArgumentGuard.NotNullOrEmpty(id, nameof(id));
        var filter = Builders<TDocument>.Filter.Eq(d => d.Id, id);
        var update = Builders<TDocument>.Update
            .Set(d => d.DeletedAt, DateTimeHelper.UtcNow)
            .Set(d => d.UpdatedAt, DateTimeHelper.UtcNow);
        await _collection.UpdateOneAsync(filter, update, null, cancellationToken);
    }

    public async Task HardDeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        ArgumentGuard.NotNullOrEmpty(id, nameof(id));
        var filter = Builders<TDocument>.Filter.Eq(d => d.Id, id);
        await _collection.DeleteOneAsync(filter, cancellationToken);
    }

    public async Task<long> CountAsync(
        Expression<Func<TDocument, bool>>? filter = null,
        CancellationToken cancellationToken = default)
    {
        var combinedFilter = NotDeletedFilter.CombineWithExpression(filter);
        return await _collection.CountDocumentsAsync(combinedFilter, null, cancellationToken);
    }

    public async Task<bool> AnyAsync(
        Expression<Func<TDocument, bool>> filter,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(filter);
        return await CountAsync(filter, cancellationToken) > 0;
    }

    public async Task RestoreAsync(string id, CancellationToken cancellationToken = default)
    {
        ArgumentGuard.NotNullOrEmpty(id, nameof(id));
        var filter = Builders<TDocument>.Filter.Eq(d => d.Id, id);
        var update = Builders<TDocument>.Update
            .Set(d => d.DeletedAt, (DateTime?)null)
            .Set(d => d.UpdatedAt, DateTimeHelper.UtcNow);
        await _collection.UpdateOneAsync(filter, update, null, cancellationToken);
    }

    // ─── Helpers ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Derive the MongoDB collection name from the document type.
    /// "ClinicalNote" → "clinical-notes".
    /// </summary>
    private static string GetCollectionName()
    {
        var typeName = typeof(TDocument).Name;
        // Convert PascalCase to kebab-case and pluralise
        var kebab = string.Concat(typeName.Select((c, i) =>
            i > 0 && char.IsUpper(c) ? "-" + char.ToLower(c) : char.ToLower(c).ToString()));
        return kebab + "s";
    }
}

