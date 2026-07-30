#nullable enable

using EHRPlatform.Common.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EHRPlatform.Common.Data.Contexts.EntityTypeConfiguration;

/// <summary>
/// Base EF Core configuration for entities inheriting from AuditableEntity.
/// Extends BaseEntityConfiguration with additional HIPAA compliance fields.
/// </summary>
/// <typeparam name="TEntity">The entity type to configure.</typeparam>
public abstract class AuditableEntityConfiguration<TEntity> : BaseEntityConfiguration<TEntity>
    where TEntity : AuditableEntity
{
    /// <summary>
    /// Configure the entity with HIPAA-compliant audit fields.
    /// </summary>
    public override void Configure(EntityTypeBuilder<TEntity> builder)
    {
        base.Configure(builder);

        // PII tracking
        builder.Property(x => x.ContainsPII)
            .IsRequired()
            .HasDefaultValue(true);

        // Access level for audit visibility
        builder.Property(x => x.AccessLevel)
            .IsRequired()
            .HasConversion<int>()
            .HasDefaultValue(AuditAccessLevel.Standard);

        // Change reason for compliance
        builder.Property(x => x.ChangeReason)
            .IsRequired(false)
            .HasMaxLength(500);

        // Source IP for audit trail
        builder.Property(x => x.SourceIPAddress)
            .IsRequired(false)
            .HasMaxLength(45); // IPv6 length

        // Encryption status
        builder.Property(x => x.IsEncrypted)
            .IsRequired()
            .HasDefaultValue(true);

        // Optimistic concurrency
        builder.Property(x => x.Version)
            .IsRequired()
            .HasDefaultValue(1)
            .IsConcurrencyToken();

        // Tenant ID for multi-tenant systems
        builder.Property(x => x.TenantId)
            .IsRequired(false);

        // Archive date for retention policies
        builder.Property(x => x.ArchivedAt)
            .IsRequired(false);

        // Indexes for compliance queries
        builder.HasIndex(x => x.ContainsPII)
            .HasDatabaseName($"IX_{typeof(TEntity).Name}_ContainsPII");

        builder.HasIndex(x => x.AccessLevel)
            .HasDatabaseName($"IX_{typeof(TEntity).Name}_AccessLevel");

        builder.HasIndex(x => x.TenantId)
            .HasDatabaseName($"IX_{typeof(TEntity).Name}_TenantId");

        builder.HasIndex(x => x.IsEncrypted)
            .HasDatabaseName($"IX_{typeof(TEntity).Name}_IsEncrypted");

        builder.HasIndex(x => x.ArchivedAt)
            .HasDatabaseName($"IX_{typeof(TEntity).Name}_ArchivedAt");
    }
}

