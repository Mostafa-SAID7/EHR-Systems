using Microsoft.EntityFrameworkCore;
using EHRPlatform.Services.FileStorage.Domain.Entities;

namespace EHRPlatform.Services.FileStorage.Persistence;

/// <summary>
/// DbContext for FileStorage Service - PostgreSQL database.
/// Manages StoredDocument, DocumentVersion, VirusScanResult, DocumentAccess entities.
/// HIPAA Compliant: Full audit logging for all document access.
/// </summary>
public class FileStorageContext : DbContext
{
    public FileStorageContext(DbContextOptions<FileStorageContext> options) : base(options) { }

    public DbSet<StoredDocument> StoredDocuments { get; set; } = null!;
    public DbSet<DocumentVersion> DocumentVersions { get; set; } = null!;
    public DbSet<VirusScanResult> VirusScanResults { get; set; } = null!;
    public DbSet<DocumentAccess> DocumentAccesses { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure StoredDocument aggregate
        modelBuilder.Entity<StoredDocument>(b =>
        {
            b.ToTable("StoredDocuments", schema: "filestorage");
            b.HasKey(x => x.Id);
            b.Property(x => x.FileName).IsRequired().HasMaxLength(500);
            b.Property(x => x.ContentType).IsRequired().HasMaxLength(100);
            b.Property(x => x.FileHash).IsRequired().HasMaxLength(256); // SHA-256
            b.Property(x => x.S3Key).IsRequired().HasMaxLength(1000);
            b.Property(x => x.S3Bucket).IsRequired().HasMaxLength(255);
            b.Property(x => x.Status).IsRequired().HasMaxLength(50);
            b.Property(x => x.Classification).IsRequired().HasMaxLength(50);
            b.Property(x => x.Category).IsRequired().HasMaxLength(50);
            b.HasMany(x => x.Versions).WithOne(x => x.Document).HasForeignKey(x => x.DocumentId).OnDelete(DeleteBehavior.Cascade);
            b.HasMany(x => x.VirusScanResults).WithOne(x => x.Document).HasForeignKey(x => x.DocumentId).OnDelete(DeleteBehavior.Cascade);
            b.HasMany(x => x.AccessHistory).WithOne(x => x.Document).HasForeignKey(x => x.DocumentId).OnDelete(DeleteBehavior.Cascade);
            b.HasIndex(x => x.PatientId);
            b.HasIndex(x => x.FileHash).IsUnique();
            b.HasIndex(x => x.Status);
            b.HasIndex(x => x.CreatedAt);
        });

        // Configure DocumentVersion
        modelBuilder.Entity<DocumentVersion>(b =>
        {
            b.ToTable("DocumentVersions", schema: "filestorage");
            b.HasKey(x => x.Id);
            b.Property(x => x.S3Key).IsRequired().HasMaxLength(1000);
            b.Property(x => x.FileHash).IsRequired().HasMaxLength(256);
            b.HasIndex(x => x.DocumentId);
        });

        // Configure VirusScanResult
        modelBuilder.Entity<VirusScanResult>(b =>
        {
            b.ToTable("VirusScanResults", schema: "filestorage");
            b.HasKey(x => x.Id);
            b.Property(x => x.ScannerName).IsRequired().HasMaxLength(100);
            b.Property(x => x.Result).IsRequired().HasMaxLength(50);
            b.Property(x => x.ThreatName).HasMaxLength(500);
            b.HasIndex(x => x.DocumentId);
        });

        // Configure DocumentAccess (audit log)
        modelBuilder.Entity<DocumentAccess>(b =>
        {
            b.ToTable("DocumentAccesses", schema: "filestorage");
            b.HasKey(x => x.Id);
            b.Property(x => x.AccessType).IsRequired().HasMaxLength(50);
            b.Property(x => x.IpAddress).IsRequired().HasMaxLength(100);
            b.HasIndex(x => x.DocumentId);
            b.HasIndex(x => x.AccessedBy);
            b.HasIndex(x => x.AccessedAt);
        });
    }
}
