using EHRPlatform.Common.Behaviors;
using EHRPlatform.Common.CQRS;
using EHRPlatform.Services.Billing.Application.Invoicing.Responses;

namespace EHRPlatform.Services.Billing.Features.Invoicing.Queries;

/// <summary>
/// Get invoice by InvoiceNumber - CACHED query.
/// InvoiceNumber is unique and provides URL-friendly slug-based access.
/// Enables slug-based routes: GET /api/v1/billing/invoices/invoice-number/{invoiceNumberSlug}
/// </summary>
public record GetInvoiceByNumberQuery : IQuery<InvoiceResponseDto>, ICachedQuery
{
    /// <summary>
    /// Invoice number (e.g., "INV-20250115-001234")
    /// </summary>
    public string InvoiceNumber { get; init; } = string.Empty;

    public string CacheKey => $"invoice_number_{InvoiceNumber.ToLower().Replace(" ", "_")}";
    public TimeSpan? Duration => TimeSpan.FromSeconds(600);
}
