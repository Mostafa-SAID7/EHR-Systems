using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using EHRPlatform.Services.Analytics.Domain.Entities;

namespace EHRPlatform.Services.Analytics.Data.Configuration;

/// <summary>
/// Entity configuration for AnalyticsMetric.
/// </summary>
public class AnalyticsMetricConfiguration : IEntityTypeConfiguration<AnalyticsMetric>
{
    public void Configure(EntityTypeBuilder<AnalyticsMetric> entity)
    {
        entity.HasKey(x => x.Id);

        // ── Single-column indexes ─────────────────────────────────────────────
        entity.HasIndex(x => x.MetricName);
        entity.HasIndex(x => x.Category);
        entity.HasIndex(x => x.RecordedAt).IsDescending();

        // ── Time-range query index ────────────────────────────────────────────
        // "Show metric X between date A and B" is the primary analytics read pattern.
        // Without a composite, Postgres evaluates MetricName and PeriodStart separately.
        entity.HasIndex(x => new { x.PeriodStart, x.PeriodEnd });
        entity.HasIndex(x => new { x.MetricName, x.RecordedAt })
              .HasDatabaseName("IX_AnalyticsMetrics_MetricName_RecordedAt")
              .IsDescending(false, true);           // MetricName ASC, RecordedAt DESC
        entity.HasIndex(x => new { x.Category, x.RecordedAt })
              .HasDatabaseName("IX_AnalyticsMetrics_Category_RecordedAt")
              .IsDescending(false, true);

        // ── Bounded string fields ────────────────────────────────────────────
        entity.Property(x => x.MetricName).HasMaxLength(200);
        entity.Property(x => x.Category).HasMaxLength(100);
        entity.Property(x => x.Unit).HasMaxLength(50);
        entity.Property(x => x.Frequency).HasMaxLength(50);

        // ── Precision for decimal metric values ─────────────────────────────
        entity.Property(x => x.Value).HasPrecision(18, 6);
    }
}
