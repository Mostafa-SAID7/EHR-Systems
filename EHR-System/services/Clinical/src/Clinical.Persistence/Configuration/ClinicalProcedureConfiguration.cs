using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using EHRPlatform.Services.Clinical.Domain.Entities;

namespace EHRPlatform.Services.Clinical.Persistence.Configuration;

/// <summary>
/// Entity configuration for ClinicalProcedure.
/// </summary>
public class ClinicalProcedureConfiguration : IEntityTypeConfiguration<ClinicalProcedure>
{
    public void Configure(EntityTypeBuilder<ClinicalProcedure> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ClinicalNoteId)
            .IsRequired();

        builder.Property(x => x.ProcedureName)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.ProcedureCode)
            .HasMaxLength(50) // CPT or SNOMED
            .IsRequired();

        builder.Property(x => x.PerformedAt)
            .IsRequired();

        builder.Property(x => x.Result)
            .HasColumnType("text");

        // Indexes
        builder.HasIndex(x => x.ClinicalNoteId);
        builder.HasIndex(x => x.ProcedureCode);
        builder.HasIndex(x => x.PerformedAt);
    }
}
