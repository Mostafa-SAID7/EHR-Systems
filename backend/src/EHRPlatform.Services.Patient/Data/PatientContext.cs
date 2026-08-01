using Microsoft.EntityFrameworkCore;
using EHRPlatform.Services.Patient.Domain.Entities;

namespace EHRPlatform.Services.Patient.Data
{
    /// <summary>
    /// Service-Specific DbContext for Patient Service
    /// Manages Patient entity and all related entities (Allergies, Conditions, Contacts, etc.).
    /// This context is ONLY used by the Patient Service.
    /// Other services do NOT reference this context.
    /// </summary>
    public class PatientContext : DbContext
    {
        public PatientContext(DbContextOptions<PatientContext> options)
            : base(options)
        {
        }

        // ─────────────────────────────────────────────────────────────────────────
        // DbSets - Entity Collections
        // ─────────────────────────────────────────────────────────────────────────

        /// <summary>Patient master data</summary>
        public DbSet<Patient> Patients { get; set; }

        /// <summary>Patient contact/address information</summary>
        public DbSet<PatientContact> PatientContacts { get; set; }

        /// <summary>Patient allergies</summary>
        public DbSet<PatientAllergy> PatientAllergies { get; set; }

        /// <summary>Patient medical conditions/diagnoses</summary>
        public DbSet<PatientCondition> PatientConditions { get; set; }

        /// <summary>Patient insurance information</summary>
        public DbSet<PatientInsurance> PatientInsurance { get; set; }

        /// <summary>Patient emergency contact information</summary>
        public DbSet<PatientEmergencyContact> PatientEmergencyContacts { get; set; }

        /// <summary>Patient medical history summary (1-to-1)</summary>
        public DbSet<PatientMedicalHistory> PatientMedicalHistories { get; set; }

        // ─────────────────────────────────────────────────────────────────────────
        // OnModelCreating - Entity Configuration
        // ─────────────────────────────────────────────────────────────────────────

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ─────────────────────────────────────────────────────────────────────
            // Patient Entity Configuration
            // ─────────────────────────────────────────────────────────────────────
            modelBuilder.Entity<Patient>(entity =>
            {
                entity.ToTable("Patients");
                entity.HasKey(e => e.Id);

                // Properties
                entity.Property(e => e.MedicalRecordNumber)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.FirstName)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.LastName)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.MiddleName)
                    .HasMaxLength(100);

                entity.Property(e => e.Gender)
                    .HasMaxLength(20);

                entity.Property(e => e.Email)
                    .HasMaxLength(255);

                entity.Property(e => e.PhoneNumber)
                    .HasMaxLength(20);

                entity.Property(e => e.Status)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasDefaultValue("Active");

                entity.Property(e => e.CreatedAt)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                // Indexes
                entity.HasIndex(e => e.MedicalRecordNumber)
                    .IsUnique()
                    .HasName("IX_Patients_MRN_Unique");

                entity.HasIndex(e => e.Email)
                    .HasName("IX_Patients_Email");

                entity.HasIndex(e => e.PhoneNumber)
                    .HasName("IX_Patients_PhoneNumber");

                entity.HasIndex(e => e.Status)
                    .HasName("IX_Patients_Status");

                entity.HasIndex(e => e.CreatedAt)
                    .HasName("IX_Patients_CreatedAt");

                entity.HasIndex(e => e.DeletedAt)
                    .HasName("IX_Patients_DeletedAt");

                // Soft delete filter
                entity.HasQueryFilter(e => e.DeletedAt == null);

                // Relationships
                entity.HasMany(e => e.Contacts)
                    .WithOne(c => c.Patient)
                    .HasForeignKey(c => c.PatientId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(e => e.Allergies)
                    .WithOne(a => a.Patient)
                    .HasForeignKey(a => a.PatientId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(e => e.Conditions)
                    .WithOne(c => c.Patient)
                    .HasForeignKey(c => c.PatientId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(e => e.InsuranceInformation)
                    .WithOne(i => i.Patient)
                    .HasForeignKey(i => i.PatientId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(e => e.EmergencyContacts)
                    .WithOne(ec => ec.Patient)
                    .HasForeignKey(ec => ec.PatientId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.MedicalHistory)
                    .WithOne(mh => mh.Patient)
                    .HasForeignKey<PatientMedicalHistory>(mh => mh.PatientId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ─────────────────────────────────────────────────────────────────────
            // PatientContact Entity Configuration
            // ─────────────────────────────────────────────────────────────────────
            modelBuilder.Entity<PatientContact>(entity =>
            {
                entity.ToTable("PatientContacts");
                entity.HasKey(e => e.Id);

                entity.Property(e => e.AddressLine1)
                    .HasMaxLength(255);

                entity.Property(e => e.AddressLine2)
                    .HasMaxLength(255);

                entity.Property(e => e.City)
                    .HasMaxLength(100);

                entity.Property(e => e.State)
                    .HasMaxLength(50);

                entity.Property(e => e.PostalCode)
                    .HasMaxLength(20);

                entity.Property(e => e.Country)
                    .HasMaxLength(100);

                entity.Property(e => e.CreatedAt)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                // Indexes
                entity.HasIndex(e => e.PatientId)
                    .HasName("IX_PatientContacts_PatientId");
            });

            // ─────────────────────────────────────────────────────────────────────
            // PatientAllergy Entity Configuration
            // ─────────────────────────────────────────────────────────────────────
            modelBuilder.Entity<PatientAllergy>(entity =>
            {
                entity.ToTable("PatientAllergies");
                entity.HasKey(e => e.Id);

                entity.Property(e => e.AllergenName)
                    .IsRequired()
                    .HasMaxLength(255);

                entity.Property(e => e.AllergenType)
                    .HasMaxLength(50);

                entity.Property(e => e.Severity)
                    .HasMaxLength(50);

                entity.Property(e => e.Reaction)
                    .HasMaxLength(1000);

                entity.Property(e => e.CreatedAt)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                // Indexes
                entity.HasIndex(e => e.PatientId)
                    .HasName("IX_PatientAllergies_PatientId");

                entity.HasIndex(e => e.IsCurrent)
                    .HasName("IX_PatientAllergies_IsCurrent");
            });

            // ─────────────────────────────────────────────────────────────────────
            // PatientCondition Entity Configuration
            // ─────────────────────────────────────────────────────────────────────
            modelBuilder.Entity<PatientCondition>(entity =>
            {
                entity.ToTable("PatientConditions");
                entity.HasKey(e => e.Id);

                entity.Property(e => e.ConditionName)
                    .IsRequired()
                    .HasMaxLength(255);

                entity.Property(e => e.ICD10Code)
                    .HasMaxLength(20);

                entity.Property(e => e.Status)
                    .HasMaxLength(50);

                entity.Property(e => e.Notes)
                    .HasMaxLength(2000);

                entity.Property(e => e.CreatedAt)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                // Indexes
                entity.HasIndex(e => e.PatientId)
                    .HasName("IX_PatientConditions_PatientId");

                entity.HasIndex(e => e.ICD10Code)
                    .HasName("IX_PatientConditions_ICD10Code");
            });

            // ─────────────────────────────────────────────────────────────────────
            // PatientInsurance Entity Configuration
            // ─────────────────────────────────────────────────────────────────────
            modelBuilder.Entity<PatientInsurance>(entity =>
            {
                entity.ToTable("PatientInsurance");
                entity.HasKey(e => e.Id);

                entity.Property(e => e.InsuranceCompanyName)
                    .IsRequired()
                    .HasMaxLength(255);

                entity.Property(e => e.PolicyNumber)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.GroupNumber)
                    .HasMaxLength(100);

                entity.Property(e => e.MemberId)
                    .HasMaxLength(100);

                entity.Property(e => e.CreatedAt)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                // Indexes
                entity.HasIndex(e => e.PatientId)
                    .HasName("IX_PatientInsurance_PatientId");

                entity.HasIndex(e => e.PolicyNumber)
                    .HasName("IX_PatientInsurance_PolicyNumber");
            });

            // ─────────────────────────────────────────────────────────────────────
            // PatientEmergencyContact Entity Configuration
            // ─────────────────────────────────────────────────────────────────────
            modelBuilder.Entity<PatientEmergencyContact>(entity =>
            {
                entity.ToTable("PatientEmergencyContacts");
                entity.HasKey(e => e.Id);

                entity.Property(e => e.ContactName)
                    .IsRequired()
                    .HasMaxLength(255);

                entity.Property(e => e.Relationship)
                    .HasMaxLength(100);

                entity.Property(e => e.PhoneNumber)
                    .HasMaxLength(20);

                entity.Property(e => e.Email)
                    .HasMaxLength(255);

                entity.Property(e => e.CreatedAt)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                // Indexes
                entity.HasIndex(e => e.PatientId)
                    .HasName("IX_PatientEmergencyContacts_PatientId");
            });

            // ─────────────────────────────────────────────────────────────────────
            // PatientMedicalHistory Entity Configuration
            // ─────────────────────────────────────────────────────────────────────
            modelBuilder.Entity<PatientMedicalHistory>(entity =>
            {
                entity.ToTable("PatientMedicalHistories");
                entity.HasKey(e => e.Id);

                // Unique constraint on PatientId (1-to-1 relationship)
                entity.HasIndex(e => e.PatientId)
                    .IsUnique()
                    .HasName("IX_PatientMedicalHistories_PatientId_Unique");

                entity.Property(e => e.BloodType)
                    .HasMaxLength(20);

                entity.Property(e => e.Height)
                    .HasPrecision(5, 2);

                entity.Property(e => e.Weight)
                    .HasPrecision(7, 2);

                entity.Property(e => e.SurgicalHistory)
                    .HasMaxLength(2000);

                entity.Property(e => e.FamilyHistory)
                    .HasMaxLength(2000);

                entity.Property(e => e.SocialHistory)
                    .HasMaxLength(2000);

                entity.Property(e => e.LastUpdatedAt)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");
            });
        }
    }
}
