#nullable enable

using EHRPlatform.Common.Domain.Entities;
using EHRPlatform.Common.Shared.Utilities.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace EHRPlatform.Common.Data.Contexts.Interceptors;

/// <summary>
/// Interceptor for managing timestamps and audit fields.
/// Sets CreatedAt on insert, UpdatedAt on update.
/// Single responsibility: Manage entity timestamps only.
/// </summary>
public sealed class AuditingInterceptor : SaveChangesInterceptor
{
    /// <summary>
    /// Async hook for setting timestamps before save.
    /// </summary>
    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is not DbContext context)
            return await base.SavingChangesAsync(eventData, result, cancellationToken);

        var entries = context.ChangeTracker.Entries<BaseEntity>().ToList();

        var now = DateTimeHelper.UtcNow;

        foreach (var entry in entries)
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = now;
                    entry.Entity.UpdatedAt = now;
                    break;

                case EntityState.Modified:
                    entry.Entity.UpdatedAt = now;
                    break;
            }

            // Handle AuditableEntity
            if (entry.Entity is AuditableEntity auditableEntity)
            {
                // CreatedBy and ModifiedBy should be set by application
                // (via ICurrentUserService in handler context)
                // Only auto-set if not already set
                if (auditableEntity.CreatedBy == Guid.Empty && entry.State == EntityState.Added)
                {
                    auditableEntity.CreatedBy = Guid.Empty; // Will be set by app
                }
            }
        }

        return await base.SavingChangesAsync(eventData, result, cancellationToken);
    }
}
