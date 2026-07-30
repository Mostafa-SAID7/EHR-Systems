#nullable enable

using EHRPlatform.Common.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EHRPlatform.Common.Data.Contexts.EntityTypeConfiguration;

/// <summary>
/// Base EF Core configuration for all entities inheriting from BaseEntity.
/// Provides consistent handling of audit fields and soft deletes across all services.
/// </summary>
/// <typeparam name="TEntity">The entity type to configure.</typeparam>
public abstract class BaseEntityConfiguration<TEntity> : IEntityTypeConfiguration<TEntity>
    where TEntity : BaseEntity
{
    /// <summary>
    /// Configure the entity with standard audit columns and indexes.
    /// Call base.ConfigureEntity(builder) in derived classes.
    /// </summary>
    public virtual void Configure(EntityTypeBuilder<TEntity> builder)
    {
        // Primary key
        builder.HasKey(x => x.Id);

        // Audit fields
        builder.Property(x => x.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(x => x.CreatedBy)
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(x => x.UpdatedBy)
            .IsRequired(false);

        builder.Property(x => x.DeletedAt)
            .IsRequired(false);

        builder.Property(x => x.DeletedBy)
            .IsRequired(false);

        // Correlation ID for tracking
        builder.Property(x => x.CorrelationId)
            .IsRequired(false)
            .HasMaxLength(50);

        // Index for soft deletes (common query filter)
        builder.HasIndex(x => x.DeletedAt)
            .HasDatabaseName($"IX_{typeof(TEntity).Name}_DeletedAt");

        // Index for creation timestamp (audit queries)
        builder.HasIndex(x => x.CreatedAt)
            .HasDatabaseName($"IX_{typeof(TEntity).Name}_CreatedAt");

        // Index for last update (audit trail queries)
        builder.HasIndex(x => x.UpdatedAt)
            .HasDatabaseName($"IX_{typeof(TEntity).Name}_UpdatedAt");

        // Global query filter for soft deletes
        builder.HasQueryFilter(x => x.DeletedAt == null);
    }

    /// <summary>
    /// Configure property constraints. Override in derived classes for entity-specific configuration.
    /// </summary>
    protected virtual void ConfigureProperties(EntityTypeBuilder<TEntity> builder)
    {
        // Override in derived classes
    }

    /// <summary>
    /// Configure relationships. Override in derived classes for entity-specific relationships.
    /// </summary>
    protected virtual void ConfigureRelationships(EntityTypeBuilder<TEntity> builder)
    {
        // Override in derived classes
    }
}

