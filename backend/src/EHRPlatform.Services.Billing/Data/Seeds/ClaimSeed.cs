using Microsoft.EntityFrameworkCore;

namespace EHRPlatform.Services.Billing.Data.Seeds;

/// <summary>
/// Seed data for InsuranceClaim.
/// </summary>
public static class ClaimSeed
{
    public static void SeedClaims(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<InsuranceClaim>().HasData(
            new InsuranceClaim
            {
                Id = Guid.Parse("55555555-5555-5555-5555-555555555555"),
                InvoiceId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                ClaimNumber = "CLM-2024-001",
                InsuranceProvider = "BlueCross",
                SubmittedAt = new DateTime(2024, 1, 5),
                Status = ClaimStatus.Submitted.ToString(),
                CreatedAt = DateTime.UtcNow,
                CreatedBy = Guid.Empty
            }
        );
    }
}
