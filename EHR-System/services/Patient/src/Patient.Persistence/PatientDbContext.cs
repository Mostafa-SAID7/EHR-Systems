namespace EHRPlatform.Services.Patient.Persistence;

using EHRPlatform.Services.Patient.Domain.Entities;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// Database context for Patient microservice.
/// </summary>
public interface IPatientDbContext
{
    DbSet<Patient> Patients { get; }
    DbSet<PatientAllergy> PatientAllergies { get; }
    DbSet<PatientCondition> PatientConditions { get; }
    DbSet<PatientTag> PatientTags { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

public class PatientDbContext : DbContext, IPatientDbContext
{
    public DbSet<Patient> Patients { get; set; } = null!;
    public DbSet<PatientAllergy> PatientAllergies { get; set; } = null!;
    public DbSet<PatientCondition> PatientConditions { get; set; } = null!;
    public DbSet<PatientTag> PatientTags { get; set; } = null!;

    public PatientDbContext(DbContextOptions<PatientDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Patient
        modelBuilder.Entity<Patient>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Mrn).IsUnique();
            entity.HasIndex(e => e.Email).IsUnique();
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.CreatedAt);
            entity.Property(e => e.FirstName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.LastName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Email).HasMaxLength(255).IsRequired();
            entity.Property(e => e.Phone).HasMaxLength(20);
            entity.Property(e => e.Mrn).HasMaxLength(20).IsRequired();
            entity.HasMany(e => e.Allergies).WithOne(a => a.Patient).HasForeignKey(a => a.PatientId).OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(e => e.Conditions).WithOne(c => c.Patient).HasForeignKey(c => c.PatientId).OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(e => e.Tags).WithOne(t => t.Patient).HasForeignKey(t => t.PatientId).OnDelete(DeleteBehavior.Cascade);
        });

        // PatientAllergy
        modelBuilder.Entity<PatientAllergy>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.PatientId);
            entity.Property(e => e.AllergyName).HasMaxLength(255).IsRequired();
            entity.Property(e => e.AllergyCode).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Severity).HasMaxLength(20);
        });

        // PatientCondition
        modelBuilder.Entity<PatientCondition>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.PatientId);
            entity.HasIndex(e => e.ConditionCode);
            entity.Property(e => e.ConditionName).HasMaxLength(255).IsRequired();
            entity.Property(e => e.ConditionCode).HasMaxLength(20).IsRequired();
            entity.Property(e => e.Status).HasMaxLength(20);
        });

        // PatientTag
        modelBuilder.Entity<PatientTag>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.PatientId);
            entity.HasIndex(e => new { e.PatientId, e.Category });
            entity.Property(e => e.TagName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Category).HasMaxLength(50);
            entity.Property(e => e.Color).HasMaxLength(20);
        });
    }
}
