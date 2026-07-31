#nullable enable

using EHRPlatform.Common.Domain.Entities;
using EHRPlatform.Common.Shared.Utilities.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace EHRPlatform.Common.Data.Contexts.Interceptors;

/// <summary>
/// Interceptor for soft delete support.
/// Prevents hard deletion — converts DELETE to UPDATE (setting DeletedAt + UpdatedAt).
///
/// BUG FIX: Previously, AuditingInterceptor ran first and saw the entity in
/// EntityState.Deleted, so it skipped the UpdatedAt assignment. Then this
/// interceptor flipped the state to Modified but UpdatedAt was never touched.
/// Fix: always stamp UpdatedAt here so soft-delete timestamps are consistent.
///
/// Single responsibility: Convert hard deletes to soft deletes only.
/// </summary>
public sealed class SoftDeleteInterceptor : SaveChangesInterceptor
{
    /// <summary>
    /// Sync hook for soft delete conversion before save.
    /// </summary>
    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        if (eventData.Context is not DbContext context)
            return base.SavingChanges(eventData, result);

        ApplySoftDelete(context);
        return base.SavingChanges(eventData, result);
    }

    /// <summary>
    /// Async hook for soft delete conversion before save.
    /// </summary>
    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is not DbContext context)
            return await base.SavingChangesAsync(eventData, result, cancellationToken);

        ApplySoftDelete(context);
        return await base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    /// <summary>
    /// Convert hard delete to soft delete:
    /// 1. Change entity state from Deleted to Modified
    /// 2. Set DeletedAt to current timestamp
    /// 3. Set UpdatedAt to current timestamp (ensures consistency)
    /// </summary>
    private static void ApplySoftDelete(DbContext context)
    {
        var now = DateTimeHelper.UtcNow;
        var deletedEntries = context.ChangeTracker
            .Entries<BaseEntity>()
            .Where(e => e.State == EntityState.Deleted)
            .ToList();

        foreach (var entry in deletedEntries)
        {
            // Convert hard delete to soft delete and stamp both timestamps.
            entry.State = EntityState.Modified;
            entry.Entity.DeletedAt = now;
            // Ensure UpdatedAt is always current — AuditingInterceptor runs
            // first and skips Deleted entries, so we must set it here.
            entry.Entity.UpdatedAt = now;
        }
    }
}
