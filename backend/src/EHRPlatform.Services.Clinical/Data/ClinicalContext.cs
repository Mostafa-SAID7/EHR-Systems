using Microsoft.EntityFrameworkCore;
using EHRPlatform.Common.Data;
using EHRPlatform.Common.Events;
using EHRPlatform.Services.Clinical.Domain.Entities;

namespace EHRPlatform.Services.Clinical.Data;

/// <summary>
/// DbContext for Clinical Service.
/// Manages clinical notes, vitals, diagnoses, procedures.
/// Includes Outbox pattern for event publishing.
/// </summary>
public class ClinicalContext : BaseDbContext
{
    public ClinicalContext(DbContextOptions<ClinicalContext> options) : base(options) { }

    public DbSet<ClinicalNote> ClinicalNotes { get; set; } = null!;
    public DbSet<VitalSigns> VitalSigns { get; set; } = null!;
    public DbSet<ClinicalDiagnosis> ClinicalDiagnoses { get; set; } = null!;
    public DbSet<ClinicalProcedure> ClinicalProcedures { get; set; } = null!;
    public DbSet<OutboxEvent> OutboxEvents { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all configurations
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ClinicalContext).Assembly);
    }
}
