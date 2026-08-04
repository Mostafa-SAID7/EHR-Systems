using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using EHRPlatform.Services.Analytics.Domain.Entities;

namespace EHRPlatform.Services.Analytics.Persistence.EntityConfigurations;

/// <summary>
/// Entity configuration for KPISummary
/// </summary>
public class KPISummaryConfiguration : IEntityTypeConfiguration<KPISummary>
{
    public void Configure(EntityTypeBuilder<KPISummary> builder)
    {
        builder.HasKey(k => k.Id);

        builder.Property(k => k.SummaryDate)
            .IsRequired();

        builder.Property(k => k.RevenueInvoiced)
            .HasColumnType("decimal(18, 2)");

        builder.Property(k => k.RevenuePaid)
            .HasColumnType("decimal(18, 2)");

        builder.Property(k => k.OutstandingBalance)
            .HasColumnType("decimal(18, 2)");

        builder.Property(k => k.SystemUptime)
            .HasColumnType("decimal(5, 2)");

        builder.Property(k => k.AverageAppointmentDurationMinutes)
            .HasColumnType("decimal(10, 2)");

        builder.Property(k => k.AverageResponseTimeMs)
            .HasColumnType("decimal(10, 2)");

        builder.Property(k => k.CreatedAt)
            .HasDefaultValueSql("GETUTCDATE()");

        // Create indexes
        builder.HasIndex(k => k.SummaryDate);
        builder.HasIndex(k => new { k.SummaryDate });

        builder.ToTable("KPISummaries");
    }
}
