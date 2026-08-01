using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using EHRPlatform.Services.Clinical.Domain.Entities;

namespace EHRPlatform.Services.Clinical.Persistence.Configuration;

/// <summary>
/// Entity configuration for ClinicalDiagnosis.
/// </summary>
public class ClinicalDiagnosisConfiguration : IEntityTypeConfiguration<ClinicalDiagnosis>
{
    public void Configure(EntityTypeBuilder<ClinicalDiagnosis> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ClinicalNoteId)
            .IsRequired();

        builder.Property(x => x.DiagnosisCode)
            .HasMaxLength(20) // ICD-10 codes
            .IsRequired();

        builder.Property(x => x.DiagnosisText)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(x => x.DiagnosisType)
            .HasMaxLength(50)
            .IsRequired();

        // Indexes
        builder.HasIndex(x => x.ClinicalNoteId);
        builder.HasIndex(x => x.DiagnosisCode);
    }
}
