using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using EHRPlatform.Services.Analytics.Domain.Entities;

namespace EHRPlatform.Services.Analytics.Data.Configuration;

/// <summary>
/// Entity configuration for DashboardWidget.
/// </summary>
public class DashboardWidgetConfiguration : IEntityTypeConfiguration<DashboardWidget>
{
    public void Configure(EntityTypeBuilder<DashboardWidget> entity)
    {
        entity.HasKey(x => x.Id);
        entity.HasOne(x => x.Dashboard).WithMany(d => d.DashboardWidgets).HasForeignKey(x => x.DashboardId);
    }
}
