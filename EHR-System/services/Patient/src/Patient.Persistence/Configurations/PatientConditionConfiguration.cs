using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EHRPlatform.Services.Patient.Persistence.Configurations;

/// <summary>
/// Entity configuration for PatientCondition.
/// </summary>
public class PatientConditionConfiguration : IEntityTypeConfiguration<PatientCondition>
{
    public void Configure(EntityTypeBuilder<PatientCondition> entity)
    {
        entity.HasKey(e => e.Id);
        entity.HasOne(e => e.Patient)
            .WithMany(p => p.Conditions)
            .HasForeignKey(e => e.PatientId)
            .OnDelete(DeleteBehavior.Cascade);
        entity.HasIndex(e => e.PatientId);
        entity.Property(e => e.Condition).IsRequired();
        entity.Property(e => e.ICD10Code).IsRequired().HasMaxLength(10);
    }
}

