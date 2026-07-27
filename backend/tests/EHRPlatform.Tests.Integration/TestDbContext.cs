#nullable enable

using Microsoft.EntityFrameworkCore;
using EHRPlatform.Common.Data;
using EHRPlatform.Common.Tags;

namespace EHRPlatform.Tests.Integration;

/// <summary>
/// Test database context for integration tests.
/// Inherits from BaseDbContext and configures Tag and TagAssociation entities.
/// </summary>
public class TestDbContext : BaseDbContext
{
    public TestDbContext(DbContextOptions<TestDbContext> options) : base(options) { }

    public DbSet<Tag> Tags { get; set; } = null!;
    public DbSet<TagAssociation> TagAssociations { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Tag entity
        modelBuilder.Entity<Tag>()
            .HasKey(t => t.Id);

        modelBuilder.Entity<Tag>()
            .HasIndex(t => t.Slug)
            .IsUnique();

        modelBuilder.Entity<Tag>()
            .HasIndex(t => new { t.Name, t.Category })
            .IsUnique();

        modelBuilder.Entity<Tag>()
            .HasIndex(t => t.Category);

        modelBuilder.Entity<Tag>()
            .HasIndex(t => t.IsArchived);

        modelBuilder.Entity<Tag>()
            .Property(t => t.Name)
            .IsRequired()
            .HasMaxLength(200);

        modelBuilder.Entity<Tag>()
            .Property(t => t.Slug)
            .IsRequired()
            .HasMaxLength(200);

        modelBuilder.Entity<Tag>()
            .Property(t => t.Category)
            .IsRequired()
            .HasMaxLength(100);

        // Configure TagAssociation entity
        modelBuilder.Entity<TagAssociation>()
            .HasKey(ta => ta.Id);

        modelBuilder.Entity<TagAssociation>()
            .HasOne(ta => ta.Tag)
            .WithMany()
            .HasForeignKey(ta => ta.TagId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<TagAssociation>()
            .HasIndex(ta => ta.ResourceId);

        modelBuilder.Entity<TagAssociation>()
            .HasIndex(ta => new { ta.ResourceId, ta.ResourceType })
            .IsUnique(false);

        modelBuilder.Entity<TagAssociation>()
            .HasIndex(ta => ta.TagId);

        modelBuilder.Entity<TagAssociation>()
            .HasIndex(ta => new { ta.TagId, ta.ResourceId, ta.ResourceType })
            .IsUnique();

        modelBuilder.Entity<TagAssociation>()
            .Property(ta => ta.ResourceType)
            .IsRequired()
            .HasMaxLength(100);

        modelBuilder.Entity<TagAssociation>()
            .Property(ta => ta.ServiceName)
            .IsRequired()
            .HasMaxLength(100);
    }
}
