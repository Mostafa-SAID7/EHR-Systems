using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EHRPlatform.Services.Patient.Data.Configuration;

/// <summary>
/// Entity configuration for Patient.
/// </summary>
public class PatientConfiguration : IEntityTypeConfiguration<PatientEntity>
{
    public void Configure(EntityTypeBuilder<PatientEntity> entity)
    {
        entity.HasKey(e => e.Id);
        entity.HasIndex(e => e.MRN).IsUnique();
        entity.HasIndex(e => e.Email);
        entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
        entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
        entity.Property(e => e.Email).HasMaxLength(255);
        entity.Property(e => e.PhoneNumber).HasMaxLength(20);
        entity.Property(e => e.MRN).IsRequired().HasMaxLength(50);
        entity.Property(e => e.BloodType).HasMaxLength(10);
        entity.Property(e => e.Status).HasMaxLength(50).HasDefaultValue("Active");
    }
}
