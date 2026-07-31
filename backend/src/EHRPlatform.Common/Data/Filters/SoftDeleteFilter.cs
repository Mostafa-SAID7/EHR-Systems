#nullable enable

using EHRPlatform.Common.Domain.Entities;
using EHRPlatform.Common.Shared.Utilities.Helpers;
using Microsoft.EntityFrameworkCore;

namespace EHRPlatform.Common.Data.Filters;

/// <summary>
/// Soft delete filter for EF Core entities.
/// Provides query filter expressions and configuration for soft-deleted entities.
/// Single responsibility: Define soft delete filter logic only.
/// </summary>
public static class SoftDeleteFilter
{
    /// <summary>
    /// Get the soft delete query filter expression.
    /// Returns only non-deleted entities (where DeletedAt == null).
    /// Usage: builder.HasQueryFilter(SoftDeleteFilter.GetExpression());
    /// </summary>
    public static System.Linq.Expressions.Expression<Func<BaseEntity, bool>> GetExpression()
        => x => x.DeletedAt == null;

    /// <summary>
    /// Configure soft delete filter on an entity type via model builder.
    /// Automatically excludes soft-deleted entities from all queries.
    /// Usage: modelBuilder.Entity&lt;MyEntity&gt;().HasQueryFilter(SoftDeleteFilter.GetExpression());
    /// </summary>
    public static void ConfigureGlobalFilter(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<BaseEntity> builder)
    {
        builder.HasQueryFilter(GetExpression());
    }

    /// <summary>
    /// Get a where predicate for manual filtering in LINQ queries.
    /// Useful when not using EF Core global query filters.
    /// Usage: var active = entities.Where(SoftDeleteFilter.GetPredicate()).ToList();
    /// </summary>
    public static Func<BaseEntity, bool> GetPredicate()
        => x => x.DeletedAt == null;

    /// <summary>
    /// Check if an entity is soft-deleted.
    /// </summary>
    public static bool IsSoftDeleted(BaseEntity entity)
        => entity.DeletedAt != null;

    /// <summary>
    /// Create a soft delete update definition for MongoDB soft deletes.
    /// Sets DeletedAt to current time and UpdatedAt for audit trail.
    /// </summary>
    public static DateTime GetSoftDeleteTimestamp()
        => DateTimeHelper.UtcNow;

    /// <summary>
    /// Get all soft-deleted entities from a query.
    /// Useful for recovery or audit operations.
    /// Usage: var deleted = query.IgnoreQueryFilters().Where(SoftDeleteFilter.GetDeletedPredicate()).ToList();
    /// </summary>
    public static Func<BaseEntity, bool> GetDeletedPredicate()
        => x => x.DeletedAt != null;
}
