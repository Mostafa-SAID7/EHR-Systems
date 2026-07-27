using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using EHRPlatform.Services.Clinical.Domain.Entities;

namespace EHRPlatform.Services.Clinical.Data.Configuration;

/// <summary>
/// Entity configuration for VitalSigns.
/// Single Responsibility: Configure VitalSigns entity mappings and relationships.
/// </summary>
public class VitalSignsConfiguration : IEntityTypeConfiguration<VitalSigns>
{
    public void Configure(EntityTypeBuilder<VitalSigns> builder)
    {
        builder.HasKey(e => e.Id);
        
        builder.HasOne(e => e.ClinicalNote)
            .WithMany(n => n.VitalSigns)
            .HasForeignKey(e => e.ClinicalNoteId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasIndex(e => e.ClinicalNoteId);
        builder.HasIndex(e => e.RecordedAt).IsDescending();
        // Composite for "vitals timeline" queries — always filtered by note, sorted by time
        builder.HasIndex(e => new { e.ClinicalNoteId, e.RecordedAt })
               .HasDatabaseName("IX_VitalSigns_ClinicalNoteId_RecordedAt")
               .IsDescending(false, true);

        builder.Property(e => e.Temperature)
            .HasPrecision(5, 2);

        builder.Property(e => e.Weight)
            .HasPrecision(6, 2);
    }
}
