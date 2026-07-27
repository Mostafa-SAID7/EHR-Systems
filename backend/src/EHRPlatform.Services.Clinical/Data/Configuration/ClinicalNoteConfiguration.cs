using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using EHRPlatform.Services.Clinical.Domain.Entities;

namespace EHRPlatform.Services.Clinical.Data.Configuration;

/// <summary>
/// Entity configuration for ClinicalNote.
/// </summary>
public class ClinicalNoteConfiguration : IEntityTypeConfiguration<ClinicalNote>
{
    public void Configure(EntityTypeBuilder<ClinicalNote> entity)
    {
        entity.HasKey(e => e.Id);

        // ── Indexes ───────────────────────────────────────────────────────────
        entity.HasIndex(e => e.PatientId);
        entity.HasIndex(e => e.ProviderId);
        entity.HasIndex(e => e.EncounterDate).IsDescending();
        entity.HasIndex(e => e.Status);
        // Composite for "patient timeline" queries — the most common read pattern
        entity.HasIndex(e => new { e.PatientId, e.EncounterDate })
              .HasDatabaseName("IX_ClinicalNotes_PatientId_EncounterDate")
              .IsDescending(false, true); // PatientId ASC, EncounterDate DESC

        // ── Bounded fields (codes / short names) ──────────────────────────────
        entity.Property(e => e.EncounterType).HasMaxLength(50);
        entity.Property(e => e.Status).HasMaxLength(50).HasDefaultValue("Draft");

        // ── SOAP content fields — explicitly unbounded TEXT ───────────────────
        // The global BaseDbContext convention was removed. We use HasColumnType("text")
        // to be explicit: SOAP notes can be thousands of characters; truncating at
        // any fixed limit corrupts clinical records.
        entity.Property(e => e.Subjective).HasColumnType("text");
        entity.Property(e => e.Objective).HasColumnType("text");
        entity.Property(e => e.Assessment).HasColumnType("text");
        entity.Property(e => e.Plan).HasColumnType("text");

        // ── Relationships ─────────────────────────────────────────────────────
        entity.HasMany(e => e.VitalSigns).WithOne(v => v.ClinicalNote).HasForeignKey(v => v.ClinicalNoteId);
        entity.HasMany(e => e.Diagnoses).WithOne(d => d.ClinicalNote).HasForeignKey(d => d.ClinicalNoteId);
        entity.HasMany(e => e.Procedures).WithOne(p => p.ClinicalNote).HasForeignKey(p => p.ClinicalNoteId);
    }
}
