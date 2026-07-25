using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using EHRPlatform.Services.Prescription.Domain.Entities;

namespace EHRPlatform.Services.Prescription.Data.Configuration;

/// <summary>
/// PrescriptionRefill entity configuration.
/// Single Responsibility: Configure PrescriptionRefill entity mapping in EF Core.
/// Part of Data Layer (persistence mapping).
/// </summary>
public class PrescriptionRefillEntityConfiguration : IEntityTypeConfiguration<PrescriptionRefill>
{
    public void Configure(EntityTypeBuilder<PrescriptionRefill> builder)
    {
        builder.HasKey(e => e.Id);
        builder.HasIndex(e => e.PrescriptionId);
        builder.HasIndex(e => new { e.PrescriptionId, e.Status });
        
        builder.Property(e => e.Status)
            .HasMaxLength(50);
    }
}
