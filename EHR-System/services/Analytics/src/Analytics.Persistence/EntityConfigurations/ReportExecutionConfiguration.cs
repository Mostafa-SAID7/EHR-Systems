using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using EHRPlatform.Services.Analytics.Domain.Entities;

namespace EHRPlatform.Services.Analytics.Persistence.EntityConfigurations;

/// <summary>
/// Entity configuration for ReportExecution
/// </summary>
public class ReportExecutionConfiguration : IEntityTypeConfiguration<ReportExecution>
{
    public void Configure(EntityTypeBuilder<ReportExecution> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Status)
            .IsRequired()
            .HasMaxLength(50)
            .HasDefaultValue("Running");

        builder.Property(e => e.ContentType)
            .HasMaxLength(100);

        builder.Property(e => e.OutputPath)
            .HasMaxLength(500);

        builder.Property(e => e.ErrorMessage)
            .HasMaxLength(1000);

        builder.Property(e => e.ExecutedAt)
            .HasDefaultValueSql("GETUTCDATE()");

        // Create indexes
        builder.HasIndex(e => e.ReportId);
        builder.HasIndex(e => e.Status);
        builder.HasIndex(e => e.ExecutedAt);

        builder.ToTable("ReportExecutions");
    }
}
