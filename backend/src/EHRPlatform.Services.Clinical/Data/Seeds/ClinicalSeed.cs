using Microsoft.EntityFrameworkCore;
using EHRPlatform.Services.Clinical.Domain.Entities;

namespace EHRPlatform.Services.Clinical.Data.Seeds;

/// <summary>
/// Seed data for Clinical (Clinical notes, vital signs, diagnoses, procedures).
/// </summary>
public static class ClinicalSeed
{
    public static void SeedClinical(this ModelBuilder modelBuilder)
    {
        var clinicalNoteId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var patientId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var providerId = Guid.Parse("33333333-3333-3333-3333-333333333333");
        var vitalSignsId = Guid.Parse("44444444-4444-4444-4444-444444444444");
        var diagnosisId = Guid.Parse("55555555-5555-5555-5555-555555555555");

        modelBuilder.Entity<ClinicalNote>().HasData(
            new ClinicalNote
            {
                Id = clinicalNoteId,
                PatientId = patientId,
                ProviderId = providerId,
                EncounterDate = DateTime.UtcNow,
                EncounterType = "Office Visit",
                Subjective = "Annual physical examination",
                Status = "Finalized",
                CreatedAt = DateTime.UtcNow
            }
        );

        modelBuilder.Entity<VitalSigns>().HasData(
            new VitalSigns
            {
                Id = vitalSignsId,
                ClinicalNoteId = clinicalNoteId,
                Temperature = 98.6m,
                SystolicBP = 120,
                DiastolicBP = 80,
                HeartRate = 72,
                RespiratoryRate = 16,
                RecordedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            }
        );

        modelBuilder.Entity<ClinicalDiagnosis>().HasData(
            new ClinicalDiagnosis
            {
                Id = diagnosisId,
                ClinicalNoteId = clinicalNoteId,
                DiagnosisCode = "Z00.00",
                DiagnosisText = "Encounter for general adult medical examination",
                DiagnosisType = "Primary",
                CreatedAt = DateTime.UtcNow
            }
        );
    }
}
