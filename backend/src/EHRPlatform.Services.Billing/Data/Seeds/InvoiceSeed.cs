using Microsoft.EntityFrameworkCore;

namespace EHRPlatform.Services.Billing.Data.Seeds;

/// <summary>
/// Seed data for Invoice and related LineItems.
/// </summary>
public static class InvoiceSeed
{
    public static void SeedInvoices(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Invoice>().HasData(
            new Invoice
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                PatientId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                InvoiceNumber = "INV-2024-001",
                ServiceDate = new DateTime(2024, 1, 1),
                DueDate = new DateTime(2024, 2, 1),
                TotalAmount = 1500.00m,
                Status = InvoiceStatus.Pending.ToString(),
                CreatedAt = DateTime.UtcNow,
                CreatedBy = Guid.Empty
            }
        );

        modelBuilder.Entity<LineItem>().HasData(
            new LineItem
            {
                Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                InvoiceId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                CPTCode = "99213",
                Description = "Office visit",
                Amount = 1500.00m,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = Guid.Empty
            }
        );
    }
}
