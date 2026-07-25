using Microsoft.EntityFrameworkCore;
using EHRPlatform.Common.Data;
using EHRPlatform.Common.Events;

namespace EHRPlatform.Services.Patient.Data;

/// <summary>
/// DbContext for Patient Service.
/// Manages patients, allergies, conditions, and the transactional outbox.
/// </summary>
public class PatientContext : BaseDbContext
{
    public PatientContext(DbContextOptions<PatientContext> options) : base(options) { }

    public DbSet<PatientEntity> Patients { get; set; } = null!;
    public DbSet<PatientAllergy> PatientAllergies { get; set; } = null!;
    public DbSet<PatientCondition> PatientConditions { get; set; } = null!;

    /// <summary>
    /// Transactional outbox — integration events awaiting publication to Kafka/RabbitMQ.
    /// Written atomically with domain changes to guarantee at-least-once delivery.
    /// </summary>
    public DbSet<OutboxEvent> OutboxEvents { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply configurations
        modelBuilder.ApplyConfiguration(new Configuration.PatientConfiguration());
        modelBuilder.ApplyConfiguration(new Configuration.PatientAllergyConfiguration());
        modelBuilder.ApplyConfiguration(new Configuration.PatientConditionConfiguration());

        // Apply seeds
        modelBuilder.SeedPatients();
    }
}
