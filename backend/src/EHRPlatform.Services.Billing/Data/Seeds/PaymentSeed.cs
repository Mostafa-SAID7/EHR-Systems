using Microsoft.EntityFrameworkCore;

namespace EHRPlatform.Services.Billing.Data.Seeds;

/// <summary>
/// Seed data for Payment transactions.
/// </summary>
public static class PaymentSeed
{
    public static void SeedPayments(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Payment>().HasData(
            new Payment
            {
                Id = Guid.Parse("44444444-4444-4444-4444-444444444444"),
                InvoiceId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                Amount = 750.00m,
                Method = PaymentMethod.CreditCard.ToString(),
                ReceivedAt = new DateTime(2024, 1, 15),
                CreatedAt = DateTime.UtcNow,
                CreatedBy = Guid.Empty
            }
        );
    }
}
