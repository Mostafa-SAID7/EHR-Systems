using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using EHRPlatform.Services.Prescription.Domain.Entities;

namespace EHRPlatform.Services.Prescription.Data.Configuration;

/// <summary>
/// Prescription entity configuration.
/// Single Responsibility: Configure Prescription entity mapping in EF Core.
/// Part of Data Layer (persistence mapping).
/// </summary>
public class PrescriptionEntityConfiguration : IEntityTypeConfiguration<PrescriptionEntity>
{
    public void Configure(EntityTypeBuilder<PrescriptionEntity> builder)
    {
        builder.HasKey(e => e.Id);
        builder.HasIndex(e => e.PatientId);
        builder.HasIndex(e => e.ProviderId);
        builder.HasIndex(e => e.Status);
        builder.HasIndex(e => e.StartDate).IsDescending();
        
        builder.Property(e => e.MedicationName)
            .IsRequired()
            .HasMaxLength(255);
        
        builder.Property(e => e.Status)
            .HasMaxLength(50)
            .HasDefaultValue("Active");
        
        builder.Property(e => e.FormType)
            .HasMaxLength(50);

        // Relationships
        builder.HasMany(e => e.Refills)
            .WithOne(r => r.Prescription)
            .HasForeignKey(r => r.PrescriptionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
