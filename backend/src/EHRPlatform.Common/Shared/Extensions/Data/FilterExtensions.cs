#nullable enable

using EHRPlatform.Common.Data.Models;
using EHRPlatform.Common.Domain.Entities;
using Microsoft.EntityFrameworkCore.Query;
using MongoDB.Driver;

namespace EHRPlatform.Common.Data.Filters;

/// <summary>
/// Extension methods for applying filters to queries.
/// Provides fluent API for combining filters across EF Core and MongoDB repositories.
/// Single responsibility: Provide filter extension methods only.
/// </summary>
public static class FilterExtensions
{
    /// <summary>
    /// Filter out soft-deleted entities from an IQueryable sequence.
    /// Usage: query.ExcludeSoftDeleted().ToList();
    /// </summary>
    public static IQueryable<T> ExcludeSoftDeleted<T>(this IQueryable<T> query)
        where T : BaseEntity
        => query.Where(SoftDeleteFilter.GetExpression());

    /// <summary>
    /// Filter to only soft-deleted entities from an IQueryable sequence.
    /// Usage: query.OnlySoftDeleted().ToList();
    /// </summary>
    public static IQueryable<T> OnlySoftDeleted<T>(this IQueryable<T> query)
        where T : BaseEntity
        => query.Where(SoftDeleteFilter.GetDeletedPredicate());

    /// <summary>
    /// Filter MongoDB query to exclude soft-deleted documents.
    /// Usage: collection.Find(query.ExcludeSoftDeleted()).ToList();
    /// </summary>
    public static FilterDefinition<TDocument> ExcludeSoftDeleted<TDocument>(
        this FilterDefinition<TDocument>? filter)
        where TDocument : MongoBaseDocument
        => NotDeletedFilter.CombineWith(filter);

    /// <summary>
    /// Filter MongoDB query to exclude soft-deleted documents from expression.
    /// Usage: collection.Find(expression.ExcludeSoftDeleted()).ToList();
    /// </summary>
    public static FilterDefinition<TDocument> ExcludeSoftDeleted<TDocument>(
        this System.Linq.Expressions.Expression<Func<TDocument, bool>>? filter)
        where TDocument : MongoBaseDocument
        => NotDeletedFilter.CombineWithExpression(filter);

    /// <summary>
    /// Filter MongoDB query to only include soft-deleted documents.
    /// Usage: collection.Find(filter.OnlySoftDeleted()).ToList();
    /// </summary>
    public static FilterDefinition<TDocument> OnlySoftDeleted<TDocument>(
        this FilterDefinition<TDocument>? filter)
        where TDocument : MongoBaseDocument
        => Builders<TDocument>.Filter.And(
            NotDeletedFilter.GetDeleted<TDocument>(),
            filter ?? Builders<TDocument>.Filter.Empty);

    /// <summary>
    /// Get a filter for a specific document ID, automatically excluding soft-deleted.
    /// Usage: collection.Find(filter.ById(id)).FirstOrDefault();
    /// </summary>
    public static FilterDefinition<TDocument> ById<TDocument>(
        this FilterDefinition<TDocument>? _,
        string id)
        where TDocument : MongoBaseDocument
        => NotDeletedFilter.GetById<TDocument>(id);

    /// <summary>
    /// Get a filter for a specific entity ID, automatically excluding soft-deleted.
    /// Usage: collection.Find(filter.ByEntityId(entityId)).FirstOrDefault();
    /// </summary>
    public static FilterDefinition<TDocument> ByEntityId<TDocument>(
        this FilterDefinition<TDocument>? _,
        Guid entityId)
        where TDocument : MongoBaseDocument
        => NotDeletedFilter.GetByEntityId<TDocument>(entityId);

    /// <summary>
    /// Apply a soft delete to an entity by setting DeletedAt.
    /// Usage: entity.MarkAsDeleted();
    /// </summary>
    public static void MarkAsDeleted(this BaseEntity entity)
    {
        entity.DeletedAt = EHRPlatform.Common.Shared.Utilities.Helpers.DateTimeHelper.UtcNow;
    }

    /// <summary>
    /// Restore a soft-deleted entity by clearing DeletedAt.
    /// Usage: entity.Restore();
    /// </summary>
    public static void Restore(this BaseEntity entity)
    {
        entity.DeletedAt = null;
    }

    /// <summary>
    /// Check if entity is soft-deleted.
    /// Usage: if (entity.IsDeleted()) { ... }
    /// </summary>
    public static bool IsDeleted(this BaseEntity entity)
        => SoftDeleteFilter.IsSoftDeleted(entity);

    /// <summary>
    /// Check if entity is NOT soft-deleted (active).
    /// Usage: if (entity.IsActive()) { ... }
    /// </summary>
    public static bool IsActive(this BaseEntity entity)
        => !SoftDeleteFilter.IsSoftDeleted(entity);
}
