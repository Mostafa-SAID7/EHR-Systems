using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using EHRPlatform.Services.Clinical.Domain.Entities;

namespace EHRPlatform.Services.Clinical.Persistence.Configuration;

/// <summary>
/// Entity configuration for VitalSigns.
/// </summary>
public class VitalSignsConfiguration : IEntityTypeConfiguration<VitalSigns>
{
    public void Configure(EntityTypeBuilder<VitalSigns> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ClinicalNoteId)
            .IsRequired();

        builder.Property(x => x.RecordedAt)
            .IsRequired();

        builder.Property(x => x.Temperature)
            .HasPrecision(5, 2);

        builder.Property(x => x.SystolicBP)
            .IsRequired();

        builder.Property(x => x.DiastolicBP)
            .IsRequired();

        builder.Property(x => x.HeartRate)
            .IsRequired();

        builder.Property(x => x.RespiratoryRate)
            .IsRequired();

        builder.Property(x => x.Weight)
            .HasPrecision(5, 2);

        // Indexes
        builder.HasIndex(x => x.ClinicalNoteId);
        builder.HasIndex(x => x.RecordedAt);
    }
}
