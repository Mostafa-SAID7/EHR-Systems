using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using EHRPlatform.Services.Analytics.Domain.Entities;

namespace EHRPlatform.Services.Analytics.Persistence.EntityConfigurations;

/// <summary>
/// Entity configuration for Report
/// </summary>
public class ReportConfiguration : IEntityTypeConfiguration<Report>
{
    public void Configure(EntityTypeBuilder<Report> builder)
    {
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Name)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(r => r.Description)
            .HasMaxLength(1000);

        builder.Property(r => r.QueryDefinition)
            .IsRequired()
            .HasColumnType("nvarchar(max)");

        builder.Property(r => r.ReportType)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(r => r.Status)
            .IsRequired()
            .HasMaxLength(50)
            .HasDefaultValue("Active");

        builder.Property(r => r.CreatedAt)
            .HasDefaultValueSql("GETUTCDATE()");

        // Configure one-to-many relationship with ReportExecution
        builder.HasMany(r => r.Executions)
            .WithOne(e => e.Report)
            .HasForeignKey(e => e.ReportId)
            .OnDelete(DeleteBehavior.Cascade);

        // Create indexes
        builder.HasIndex(r => r.CreatedBy);
        builder.HasIndex(r => r.Status);
        builder.HasIndex(r => r.ReportType);

        builder.ToTable("Reports");
    }
}
