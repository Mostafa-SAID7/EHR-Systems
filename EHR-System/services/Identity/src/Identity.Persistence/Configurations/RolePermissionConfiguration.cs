using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using EHRPlatform.Services.Identity.Domain.Entities;

namespace Identity.Persistence.Configurations;

/// <summary>
/// Entity configuration for RolePermission (junction table).
/// </summary>
public class RolePermissionConfiguration : IEntityTypeConfiguration<RolePermission>
{
    public void Configure(EntityTypeBuilder<RolePermission> entity)
    {
        entity.HasKey(e => new { e.RoleId, e.PermissionId });
        entity.HasOne(e => e.Role).WithMany(r => r.Permissions).HasForeignKey(e => e.RoleId);
        entity.HasOne(e => e.Permission).WithMany().HasForeignKey(e => e.PermissionId);
    }
}

