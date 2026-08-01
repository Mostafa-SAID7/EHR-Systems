using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using EHRPlatform.Services.Analytics.Domain.Entities;

namespace EHRPlatform.Services.Analytics.Data.Configuration;

/// <summary>
/// Entity configuration for Dashboard.
/// </summary>
public class DashboardConfiguration : IEntityTypeConfiguration<Dashboard>
{
    public void Configure(EntityTypeBuilder<Dashboard> entity)
    {
        entity.HasKey(e => e.Id);
        entity.HasIndex(e => e.UserId);
        entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
        entity.HasMany(e => e.DashboardWidgets).WithOne(w => w.Dashboard).HasForeignKey(w => w.DashboardId);
    }
}
