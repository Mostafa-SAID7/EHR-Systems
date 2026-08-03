using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using EHRPlatform.Services.Identity.Domain.Entities;

namespace Identity.Persistence.Configurations;

/// <summary>
/// Entity configuration for MfaSetup.
/// </summary>
public class MfaSetupConfiguration : IEntityTypeConfiguration<MfaSetup>
{
    public void Configure(EntityTypeBuilder<MfaSetup> entity)
    {
        entity.HasKey(e => e.Id);
        entity.HasIndex(e => e.UserId);
    }
}

