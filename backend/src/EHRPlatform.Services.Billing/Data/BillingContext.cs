using Microsoft.EntityFrameworkCore;
using EHRPlatform.Common.Data.Contexts;
using EHRPlatform.Common.Events;

namespace EHRPlatform.Services.Billing.Data;

/// <summary>
/// DbContext for Billing Service.
/// </summary>
public class BillingContext : BaseDbContext
{
    public BillingContext(DbContextOptions<BillingContext> options) : base(options) { }

    public DbSet<Invoice> Invoices { get; set; } = null!;
    public DbSet<LineItem> LineItems { get; set; } = null!;
    public DbSet<Payment> Payments { get; set; } = null!;
    public DbSet<InsuranceClaim> InsuranceClaims { get; set; } = null!;
    
    // ✓ Outbox Event Pattern - Ensures consistency across stores
    public DbSet<OutboxEvent> OutboxEvents { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply entity configurations
        modelBuilder.ApplyConfiguration(new InvoiceConfiguration());
        modelBuilder.ApplyConfiguration(new LineItemConfiguration());
        modelBuilder.ApplyConfiguration(new PaymentConfiguration());
        modelBuilder.ApplyConfiguration(new InsuranceClaimConfiguration());

        // Apply seed data
        modelBuilder.SeedInvoices();
        modelBuilder.SeedPayments();
        modelBuilder.SeedClaims();
        modelBuilder.SeedReports();
    }
}
