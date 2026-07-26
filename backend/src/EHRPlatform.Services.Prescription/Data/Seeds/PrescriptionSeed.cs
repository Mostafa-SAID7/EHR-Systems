using Microsoft.EntityFrameworkCore;

namespace EHRPlatform.Services.Prescription.Data.Seeds;

/// <summary>
/// Seed data for Prescription and Refills.
/// </summary>
public static class PrescriptionSeed
{
    public static void SeedPrescriptions(this ModelBuilder modelBuilder)
    {
        var prescriptionId = Guid.Parse("11111111-1111-1111-1111-111111111111");

        modelBuilder.Entity<PrescriptionEntity>().HasData(
            new PrescriptionEntity
            {
                Id = prescriptionId,
                PatientId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                ProviderId = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                MedicationName = "Amoxicillin",
                Strength = "500mg",
                FormType = "Tablet",
                Dosage = "1 tablet",
                Frequency = "TID",
                Quantity = 30,
                RefillsAllowed = 2,
                StartDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                Status = "Active",
                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            }
        );

        modelBuilder.Entity<PrescriptionRefill>().HasData(
            new PrescriptionRefill
            {
                Id = Guid.Parse("44444444-4444-4444-4444-444444444444"),
                PrescriptionId = prescriptionId,
                RequestedAt = new DateTime(2026, 1, 15, 0, 0, 0, DateTimeKind.Utc),
                ApprovedAt = new DateTime(2026, 1, 16, 0, 0, 0, DateTimeKind.Utc),
                Status = "Dispensed",
                CreatedAt = new DateTime(2026, 1, 15, 0, 0, 0, DateTimeKind.Utc)
            }
        );
    }
}
