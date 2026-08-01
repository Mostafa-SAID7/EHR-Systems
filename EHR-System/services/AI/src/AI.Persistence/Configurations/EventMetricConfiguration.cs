using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using EHRPlatform.Services.Analytics.Domain.Entities;

namespace EHRPlatform.Services.Analytics.Data.Configuration;

/// <summary>
/// Entity configuration for EventMetric.
/// </summary>
public class EventMetricConfiguration : IEntityTypeConfiguration<EventMetric>
{
    public void Configure(EntityTypeBuilder<EventMetric> entity)
    {
        entity.HasKey(x => x.Id);

        // ── Single-column fallback indexes ─────────────────────────────────────
        entity.HasIndex(x => x.EventType);
        entity.HasIndex(x => x.OccurredAt).IsDescending();

        // ── Composite time-series indexes ────────────────────────────────────
        // Analytics queries almost always filter by EventType AND a time window.
        // A single-column index on OccurredAt forces a full-index scan when also
        // filtering on EventType. The composite index covers both predicates.
        entity.HasIndex(x => new { x.EventType, x.OccurredAt })
              .HasDatabaseName("IX_EventMetrics_EventType_OccurredAt")
              .IsDescending(false, true);          // EventType ASC, OccurredAt DESC

        // Aggregate-scoped time-series (e.g. "all events for patient X")
        entity.HasIndex(x => new { x.AggregateId, x.OccurredAt })
              .HasDatabaseName("IX_EventMetrics_AggregateId_OccurredAt")
              .IsDescending(false, true);

        // ── Properties — store as JSONB for Postgres-native querying ──────────
        // EventMetric.Properties is Dictionary<string,string>. Without this the
        // default EF Core provider stores it as a JSON *string* (no operators).
        // With HasColumnType("jsonb") Postgres can index and query inside the doc.
        entity.Property(x => x.Properties)
              .HasColumnType("jsonb");
    }
}
