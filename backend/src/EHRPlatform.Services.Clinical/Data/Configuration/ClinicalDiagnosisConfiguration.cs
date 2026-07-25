using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using EHRPlatform.Services.Clinical.Domain.Entities;

namespace EHRPlatform.Services.Clinical.Data.Configuration;

/// <summary>
/// Entity configuration for ClinicalDiagnosis.
/// Single Responsibility: Configure ClinicalDiagnosis entity mappings and relationships.
/// </summary>
public class ClinicalDiagnosisConfiguration : IEntityTypeConfiguration<ClinicalDiagnosis>
{
    public void Configure(EntityTypeBuilder<ClinicalDiagnosis> builder)
    {
        builder.HasKey(e => e.Id);
        
        builder.HasOne(e => e.ClinicalNote)
            .WithMany(n => n.Diagnoses)
            .HasForeignKey(e => e.ClinicalNoteId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasIndex(e => e.ClinicalNoteId);
        builder.HasIndex(e => e.DiagnosisCode);
        
        builder.Property(e => e.DiagnosisCode)
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(e => e.DiagnosisText)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(e => e.DiagnosisType)
            .HasMaxLength(50)
            .IsRequired();
    }
}
