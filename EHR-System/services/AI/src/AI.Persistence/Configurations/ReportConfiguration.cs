using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using EHRPlatform.Services.Analytics.Domain.Entities;

namespace EHRPlatform.Services.Analytics.Data.Configuration;

/// <summary>
/// Entity configuration for Report.
/// </summary>
public class ReportConfiguration : IEntityTypeConfiguration<Report>
{
    public void Configure(EntityTypeBuilder<Report> entity)
    {
        entity.HasKey(e => e.Id);
        entity.HasIndex(e => e.UserId);
        entity.HasIndex(e => e.Schedule);
        entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
        entity.Property(e => e.ReportType).HasMaxLength(50);
        entity.HasMany(e => e.Executions).WithOne(e => e.Report).HasForeignKey(e => e.ReportId);
    }
}
