using Microsoft.EntityFrameworkCore;
using EHRPlatform.Services.Billing.Domain.Entities;

namespace EHRPlatform.Services.Billing.Persistence;

/// <summary>
/// DbContext for Billing Service - PostgreSQL database.
/// Manages Invoice, LineItem, Payment, InsuranceClaim, PriorAuthorization aggregates.
/// HIPAA Compliant: All changes are audited with CreatedAt/UpdatedAt timestamps.
/// </summary>
public class BillingContext : DbContext
{
    public BillingContext(DbContextOptions<BillingContext> options) : base(options) { }

    public DbSet<Invoice> Invoices { get; set; } = null!;
    public DbSet<LineItem> LineItems { get; set; } = null!;
    public DbSet<Payment> Payments { get; set; } = null!;
    public DbSet<InsuranceClaim> InsuranceClaims { get; set; } = null!;
    public DbSet<PriorAuthorization> PriorAuthorizations { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Invoice aggregate
        modelBuilder.Entity<Invoice>(b =>
        {
            b.ToTable("Invoices", schema: "billing");
            b.HasKey(x => x.Id);
            b.Property(x => x.InvoiceNumber).IsRequired().HasMaxLength(50);
            b.Property(x => x.Status).IsRequired().HasMaxLength(50);
            b.HasMany(x => x.LineItems).WithOne(x => x.Invoice).HasForeignKey(x => x.InvoiceId).OnDelete(DeleteBehavior.Cascade);
            b.HasMany(x => x.Payments).WithOne(x => x.Invoice).HasForeignKey(x => x.InvoiceId).OnDelete(DeleteBehavior.Cascade);
            b.HasMany(x => x.InsuranceClaims).WithOne(x => x.Invoice).HasForeignKey(x => x.InvoiceId).OnDelete(DeleteBehavior.Cascade);
            b.HasIndex(x => x.PatientId);
            b.HasIndex(x => x.InvoiceNumber).IsUnique();
            b.HasIndex(x => x.Status);
        });

        // Configure LineItem
        modelBuilder.Entity<LineItem>(b =>
        {
            b.ToTable("LineItems", schema: "billing");
            b.HasKey(x => x.Id);
            b.Property(x => x.CPTCode).IsRequired().HasMaxLength(10);
            b.Property(x => x.Description).IsRequired().HasMaxLength(500);
        });

        // Configure Payment
        modelBuilder.Entity<Payment>(b =>
        {
            b.ToTable("Payments", schema: "billing");
            b.HasKey(x => x.Id);
            b.Property(x => x.Method).IsRequired().HasMaxLength(50);
            b.Property(x => x.Reference).HasMaxLength(100);
            b.HasIndex(x => x.ReceivedAt);
        });

        // Configure InsuranceClaim
        modelBuilder.Entity<InsuranceClaim>(b =>
        {
            b.ToTable("InsuranceClaims", schema: "billing");
            b.HasKey(x => x.Id);
            b.Property(x => x.ClaimNumber).IsRequired().HasMaxLength(50);
            b.Property(x => x.InsuranceProvider).IsRequired().HasMaxLength(200);
            b.Property(x => x.Status).IsRequired().HasMaxLength(50);
            b.HasIndex(x => x.ClaimNumber).IsUnique();
            b.HasIndex(x => x.Status);
        });

        // Configure PriorAuthorization
        modelBuilder.Entity<PriorAuthorization>(b =>
        {
            b.ToTable("PriorAuthorizations", schema: "billing");
            b.HasKey(x => x.Id);
            b.Property(x => x.InsuranceProvider).IsRequired().HasMaxLength(200);
            b.Property(x => x.ServiceCode).IsRequired().HasMaxLength(50);
            b.Property(x => x.Status).IsRequired().HasMaxLength(50);
            b.HasIndex(x => x.PatientId);
            b.HasIndex(x => x.Status);
        });
    }
}
