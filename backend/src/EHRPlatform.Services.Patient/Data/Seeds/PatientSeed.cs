using Microsoft.EntityFrameworkCore;

namespace EHRPlatform.Services.Patient.Data.Seeds;

/// <summary>
/// Seed data for Patient, Allergies, and Conditions.
/// </summary>
public static class PatientSeed
{
    public static void SeedPatients(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PatientEntity>().HasData(
            new PatientEntity
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@example.com",
                PhoneNumber = "+1-555-0123",
                DateOfBirth = new DateTime(1990, 1, 15),
                Gender = "M",
                MRN = "MRN-001",
                BloodType = "O+",
                Status = "Active",
                CreatedAt = DateTime.UtcNow
            }
        );

        modelBuilder.Entity<PatientAllergy>().HasData(
            new PatientAllergy
            {
                Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                PatientId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                Allergen = "Penicillin",
                Severity = "Severe",
                CreatedAt = DateTime.UtcNow
            }
        );

        modelBuilder.Entity<PatientCondition>().HasData(
            new PatientCondition
            {
                Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                PatientId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                Condition = "Hypertension",
                ICD10Code = "I10",
                CreatedAt = DateTime.UtcNow
            }
        );
    }
}
