using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using EHRPlatform.Services.Analytics.Domain.Entities;

namespace EHRPlatform.Services.Analytics.Data.Configuration;

/// <summary>
/// Entity configuration for ReportExecution.
/// </summary>
public class ReportExecutionConfiguration : IEntityTypeConfiguration<ReportExecution>
{
    public void Configure(EntityTypeBuilder<ReportExecution> entity)
    {
        entity.HasKey(x => x.Id);
        entity.HasOne(x => x.Report).WithMany(r => r.Executions).HasForeignKey(x => x.ReportId);
        entity.HasIndex(x => x.ExecutedAt).IsDescending();
    }
}
