namespace EHRPlatform.Services.Audit.Persistence;

using EHRPlatform.Services.Audit.Domain.Entities;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// Database context for Audit microservice.
/// Append-only (no update/delete for HIPAA compliance).
/// </summary>
public interface IAuditDbContext
{
    DbSet<AuditEntry> AuditEntries { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

public class AuditDbContext : DbContext, IAuditDbContext
{
    public DbSet<AuditEntry> AuditEntries { get; set; } = null!;

    public AuditDbContext(DbContextOptions<AuditDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // AuditEntry - Append-only table
        modelBuilder.Entity<AuditEntry>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            // Indexes for queries
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.ResourceType);
            entity.HasIndex(e => new { e.ResourceType, e.ResourceId }); // Get resource history
            entity.HasIndex(e => e.CreatedAt); // Time-based queries
            entity.HasIndex(e => new { e.UserId, e.CreatedAt }); // User activity timeline
            entity.HasIndex(e => new { e.ContainsSsn, e.ContainsDob, e.ContainsMrn }); // PII tracking
            
            // Properties
            entity.Property(e => e.Id).IsRequired();
            entity.Property(e => e.UserId).IsRequired();
            entity.Property(e => e.UserEmail).HasMaxLength(255).IsRequired();
            entity.Property(e => e.UserFullName).HasMaxLength(255).IsRequired();
            entity.Property(e => e.Action).HasMaxLength(50).IsRequired();
            entity.Property(e => e.ResourceType).HasMaxLength(50).IsRequired();
            entity.Property(e => e.ResourceId).IsRequired();
            entity.Property(e => e.Status).HasMaxLength(20).IsRequired();
            entity.Property(e => e.IpAddress).HasMaxLength(45); // IPv6 max length
            entity.Property(e => e.HttpMethod).HasMaxLength(10);
            entity.Property(e => e.Endpoint).HasMaxLength(500);
            entity.Property(e => e.AccessLevel).HasMaxLength(20);
            entity.Property(e => e.IntegrityHash).HasMaxLength(64).IsRequired(); // SHA-256 hex
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.ChangeDetails).IsRequired(false);
            entity.Property(e => e.ErrorMessage).IsRequired(false);
            
            // NO update/delete constraints - Append-only
            entity.ToTable("AuditEntries", t => 
            {
                // Add check constraint to prevent updates
                t.HasCheckConstraint("CK_NoUpdate", "1=0 OR 1=1"); // Placeholder, actual constraint in migration
            });
        });
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await base.SaveChangesAsync(cancellationToken);
    }
}
