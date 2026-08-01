using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using EHRPlatform.Services.Clinical.Domain.Entities;

namespace EHRPlatform.Services.Clinical.Persistence.Configuration;

/// <summary>
/// Entity configuration for ClinicalNote aggregate.
/// </summary>
public class ClinicalNoteConfiguration : IEntityTypeConfiguration<ClinicalNote>
{
    public void Configure(EntityTypeBuilder<ClinicalNote> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.PatientId)
            .IsRequired();

        builder.Property(x => x.ProviderId)
            .IsRequired();

        builder.Property(x => x.EncounterDate)
            .IsRequired();

        builder.Property(x => x.EncounterType)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.Status)
            .HasMaxLength(50)
            .HasDefaultValue("Draft")
            .IsRequired();

        builder.Property(x => x.Subjective)
            .HasColumnType("text");

        builder.Property(x => x.Objective)
            .HasColumnType("text");

        builder.Property(x => x.Assessment)
            .HasColumnType("text");

        builder.Property(x => x.Plan)
            .HasColumnType("text");

        // Relationships
        builder.HasMany(x => x.VitalSigns)
            .WithOne(x => x.ClinicalNote)
            .HasForeignKey(x => x.ClinicalNoteId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Diagnoses)
            .WithOne(x => x.ClinicalNote)
            .HasForeignKey(x => x.ClinicalNoteId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Procedures)
            .WithOne(x => x.ClinicalNote)
            .HasForeignKey(x => x.ClinicalNoteId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(x => x.PatientId);
        builder.HasIndex(x => x.ProviderId);
        builder.HasIndex(x => new { x.PatientId, x.EncounterDate });
        builder.HasIndex(x => x.Status);
    }
}
