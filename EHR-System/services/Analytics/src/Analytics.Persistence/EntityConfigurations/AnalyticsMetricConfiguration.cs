using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using EHRPlatform.Services.Analytics.Domain.Entities;

namespace EHRPlatform.Services.Analytics.Persistence.EntityConfigurations;

/// <summary>
/// Entity configuration for AnalyticsMetric
/// </summary>
public class AnalyticsMetricConfiguration : IEntityTypeConfiguration<AnalyticsMetric>
{
    public void Configure(EntityTypeBuilder<AnalyticsMetric> builder)
    {
        builder.HasKey(m => m.Id);

        builder.Property(m => m.MetricName)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(m => m.Category)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(m => m.Value)
            .HasColumnType("decimal(18, 2)");

        builder.Property(m => m.Unit)
            .HasMaxLength(50);

        builder.Property(m => m.Dimension1)
            .HasMaxLength(255);

        builder.Property(m => m.Dimension2)
            .HasMaxLength(255);

        builder.Property(m => m.Dimension3)
            .HasMaxLength(255);

        builder.Property(m => m.Timestamp)
            .HasDefaultValueSql("GETUTCDATE()");

        builder.Property(m => m.CreatedAt)
            .HasDefaultValueSql("GETUTCDATE()");

        // Create indexes for queries
        builder.HasIndex(m => m.MetricName);
        builder.HasIndex(m => m.Category);
        builder.HasIndex(m => m.Timestamp);
        builder.HasIndex(m => new { m.MetricName, m.Timestamp });
        builder.HasIndex(m => new { m.Category, m.Timestamp });

        builder.ToTable("AnalyticsMetrics");
    }
}
