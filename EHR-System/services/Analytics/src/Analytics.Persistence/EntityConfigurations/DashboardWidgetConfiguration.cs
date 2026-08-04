using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using EHRPlatform.Services.Analytics.Domain.Entities;

namespace EHRPlatform.Services.Analytics.Persistence.EntityConfigurations;

/// <summary>
/// Entity configuration for DashboardWidget
/// </summary>
public class DashboardWidgetConfiguration : IEntityTypeConfiguration<DashboardWidget>
{
    public void Configure(EntityTypeBuilder<DashboardWidget> builder)
    {
        builder.HasKey(w => w.Id);

        builder.Property(w => w.WidgetType)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(w => w.MetricName)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(w => w.Title)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(w => w.Configuration)
            .HasColumnType("nvarchar(max)");

        builder.Property(w => w.Width)
            .HasDefaultValue(4);

        builder.Property(w => w.Height)
            .HasDefaultValue(2);

        builder.Property(w => w.PositionX)
            .HasDefaultValue(0);

        builder.Property(w => w.PositionY)
            .HasDefaultValue(0);

        builder.Property(w => w.CreatedAt)
            .HasDefaultValueSql("GETUTCDATE()");

        // Create indexes
        builder.HasIndex(w => w.DashboardId);
        builder.HasIndex(w => w.MetricName);

        builder.ToTable("DashboardWidgets");
    }
}
