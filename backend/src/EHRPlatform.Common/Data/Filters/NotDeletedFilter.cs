#nullable enable

using EHRPlatform.Common.Data.Models;
using MongoDB.Driver;

namespace EHRPlatform.Common.Data.Filters;

/// <summary>
/// MongoDB NotDeleted filter definition.
/// Provides reusable filter for excluding soft-deleted documents.
/// Single responsibility: Define MongoDB soft-delete filter only.
/// </summary>
public static class NotDeletedFilter
{
    /// <summary>
    /// Get the NotDeleted filter for MongoDB documents.
    /// Filters for documents where DeletedAt is null (not soft-deleted).
    /// Usage: var filter = NotDeletedFilter.Get&lt;TDocument&gt;();
    /// </summary>
    public static FilterDefinition<TDocument> Get<TDocument>()
        where TDocument : MongoBaseDocument
        => Builders<TDocument>.Filter.Eq(d => d.DeletedAt, null);

    /// <summary>
    /// Combine a user-provided filter with the NotDeleted filter using AND logic.
    /// Ensures soft-deleted documents are always excluded unless caller explicitly overrides.
    /// Usage: var combined = NotDeletedFilter.CombineWith&lt;TDocument&gt;(userFilter);
    /// </summary>
    public static FilterDefinition<TDocument> CombineWith<TDocument>(
        FilterDefinition<TDocument>? userFilter = null)
        where TDocument : MongoBaseDocument
    {
        var notDeleted = Get<TDocument>();
        return userFilter is null
            ? notDeleted
            : Builders<TDocument>.Filter.And(notDeleted, userFilter);
    }

    /// <summary>
    /// Combine a lambda filter expression with the NotDeleted filter using AND logic.
    /// Ensures soft-deleted documents are always excluded unless caller explicitly overrides.
    /// Usage: var combined = NotDeletedFilter.CombineWithExpression&lt;TDocument&gt;(d => d.Status == Active);
    /// </summary>
    public static FilterDefinition<TDocument> CombineWithExpression<TDocument>(
        System.Linq.Expressions.Expression<Func<TDocument, bool>>? filter = null)
        where TDocument : MongoBaseDocument
    {
        var notDeleted = Get<TDocument>();
        if (filter is null)
            return notDeleted;

        var filterDefinition = Builders<TDocument>.Filter.Where(filter);
        return Builders<TDocument>.Filter.And(notDeleted, filterDefinition);
    }

    /// <summary>
    /// Get the filter for finding deleted documents (DeletedAt != null).
    /// Useful for recovery or audit operations.
    /// Usage: var deleted = collection.Find(NotDeletedFilter.GetDeleted&lt;TDocument&gt;()).ToList();
    /// </summary>
    public static FilterDefinition<TDocument> GetDeleted<TDocument>()
        where TDocument : MongoBaseDocument
        => Builders<TDocument>.Filter.Ne(d => d.DeletedAt, (DateTime?)null);

    /// <summary>
    /// Get the filter for a specific ID combined with NotDeleted filter.
    /// Useful for GetById operations that should exclude deleted documents.
    /// Usage: var doc = collection.Find(NotDeletedFilter.GetById&lt;TDocument&gt;(id)).FirstOrDefault();
    /// </summary>
    public static FilterDefinition<TDocument> GetById<TDocument>(string id)
        where TDocument : MongoBaseDocument
        => Builders<TDocument>.Filter.And(
            Get<TDocument>(),
            Builders<TDocument>.Filter.Eq(d => d.Id, id));

    /// <summary>
    /// Get the filter for a specific EntityId combined with NotDeleted filter.
    /// Useful for GetByEntityId operations that should exclude deleted documents.
    /// Usage: var doc = collection.Find(NotDeletedFilter.GetByEntityId&lt;TDocument&gt;(entityId)).FirstOrDefault();
    /// </summary>
    public static FilterDefinition<TDocument> GetByEntityId<TDocument>(Guid entityId)
        where TDocument : MongoBaseDocument
        => Builders<TDocument>.Filter.And(
            Get<TDocument>(),
            Builders<TDocument>.Filter.Eq(d => d.EntityId, entityId));
}
