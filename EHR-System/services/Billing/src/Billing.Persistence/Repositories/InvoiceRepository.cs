using Microsoft.EntityFrameworkCore;
using EHRPlatform.Services.Billing.Domain.Entities;

namespace EHRPlatform.Services.Billing.Persistence.Repositories;

/// <summary>
/// Repository for Invoice entity - specialized queries for billing domain.
/// Includes invoice lookup by number, patient queries, and payment tracking.
/// </summary>
public class InvoiceRepository : GenericRepository<Invoice>
{
    public InvoiceRepository(BillingContext context) : base(context) { }

    public async Task<Invoice?> GetByInvoiceNumberAsync(string invoiceNumber, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(x => x.LineItems)
            .Include(x => x.Payments)
            .Include(x => x.InsuranceClaims)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.InvoiceNumber == invoiceNumber, cancellationToken);
    }

    public async Task<List<Invoice>> GetPatientInvoicesAsync(Guid patientId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(x => x.PatientId == patientId)
            .Include(x => x.LineItems)
            .Include(x => x.Payments)
            .AsNoTracking()
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Invoice>> GetInvoicesByStatusAsync(string status, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(x => x.Status == status)
            .Include(x => x.Payments)
            .AsNoTracking()
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<Invoice?> GetFullInvoiceAsync(Guid invoiceId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(x => x.LineItems)
            .Include(x => x.Payments)
            .Include(x => x.InsuranceClaims)
            .FirstOrDefaultAsync(x => x.Id == invoiceId, cancellationToken);
    }

    public async Task<decimal> GetPatientTotalDueAsync(Guid patientId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(x => x.PatientId == patientId && x.Status != "Cancelled")
            .SumAsync(x => x.BalanceDue, cancellationToken);
    }
}
