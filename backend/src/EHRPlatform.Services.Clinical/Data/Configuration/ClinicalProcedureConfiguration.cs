using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using EHRPlatform.Services.Clinical.Domain.Entities;

namespace EHRPlatform.Services.Clinical.Data.Configuration;

/// <summary>
/// Entity configuration for ClinicalProcedure.
/// Single Responsibility: Configure ClinicalProcedure entity mappings and relationships.
/// </summary>
public class ClinicalProcedureConfiguration : IEntityTypeConfiguration<ClinicalProcedure>
{
    public void Configure(EntityTypeBuilder<ClinicalProcedure> builder)
    {
        builder.HasKey(e => e.Id);
        
        builder.HasOne(e => e.ClinicalNote)
            .WithMany(n => n.Procedures)
            .HasForeignKey(e => e.ClinicalNoteId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasIndex(e => e.ClinicalNoteId);
        builder.HasIndex(e => e.PerformedAt).IsDescending();
        
        builder.Property(e => e.ProcedureCode)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(e => e.ProcedureName)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(e => e.Result)
            .HasMaxLength(4000);
    }
}
