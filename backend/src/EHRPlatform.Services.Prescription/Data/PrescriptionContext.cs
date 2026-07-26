using Microsoft.EntityFrameworkCore;
using EHRPlatform.Common.Data;
using EHRPlatform.Services.Prescription.Domain.Entities;

namespace EHRPlatform.Services.Prescription.Data;

/// <summary>
/// DbContext for Prescription Service.
/// Part of Data Layer (handles persistence).
/// </summary>
public class PrescriptionContext : BaseDbContext
{
    public PrescriptionContext(DbContextOptions<PrescriptionContext> options) : base(options) { }

    public DbSet<PrescriptionEntity> Prescriptions { get; set; } = null!;
    public DbSet<PrescriptionRefill> PrescriptionRefills { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all entity configurations
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PrescriptionContext).Assembly);
    }
}
