using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EHRPlatform.Services.Patient.Persistence.Configurations;

/// <summary>
/// Entity configuration for PatientAllergy.
/// </summary>
public class PatientAllergyConfiguration : IEntityTypeConfiguration<PatientAllergy>
{
    public void Configure(EntityTypeBuilder<PatientAllergy> entity)
    {
        entity.HasKey(e => e.Id);
        entity.HasOne(e => e.Patient)
            .WithMany(p => p.Allergies)
            .HasForeignKey(e => e.PatientId)
            .OnDelete(DeleteBehavior.Cascade);
        entity.HasIndex(e => e.PatientId);
        entity.Property(e => e.Allergen).IsRequired().HasMaxLength(255);
        entity.Property(e => e.Severity).HasMaxLength(50);
    }
}

