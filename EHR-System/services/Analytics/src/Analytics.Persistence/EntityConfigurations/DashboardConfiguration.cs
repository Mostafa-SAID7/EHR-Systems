using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using EHRPlatform.Services.Analytics.Domain.Entities;

namespace EHRPlatform.Services.Analytics.Persistence.EntityConfigurations;

/// <summary>
/// Entity configuration for Dashboard
/// </summary>
public class DashboardConfiguration : IEntityTypeConfiguration<Dashboard>
{
    public void Configure(EntityTypeBuilder<Dashboard> builder)
    {
        builder.HasKey(d => d.Id);

        builder.Property(d => d.Name)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(d => d.Description)
            .HasMaxLength(1000);

        builder.Property(d => d.IsPublic)
            .HasDefaultValue(false);

        builder.Property(d => d.DisplayOrder)
            .HasDefaultValue(0);

        builder.Property(d => d.CreatedAt)
            .HasDefaultValueSql("GETUTCDATE()");

        // Configure one-to-many relationship with DashboardWidget
        builder.HasMany(d => d.Widgets)
            .WithOne(w => w.Dashboard)
            .HasForeignKey(w => w.DashboardId)
            .OnDelete(DeleteBehavior.Cascade);

        // Create indexes
        builder.HasIndex(d => d.UserId);
        builder.HasIndex(d => d.CreatedAt);
        builder.HasIndex(d => d.IsPublic);

        builder.ToTable("Dashboards");
    }
}
