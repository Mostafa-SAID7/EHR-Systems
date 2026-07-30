using EHRPlatform.Common.Application.Behaviors;
using EHRPlatform.Common.Application.CQRS;
using EHRPlatform.Services.Billing.Application.Invoicing.Responses;

namespace EHRPlatform.Services.Billing.Features.Invoicing.Queries;

/// <summary>
/// Get invoice by ID - CACHED query.
/// </summary>
public record GetInvoiceQuery : IQuery<InvoiceResponseDto>, ICachedQuery
{
    public Guid InvoiceId { get; init; }

    public string CacheKey => $"invoice_{InvoiceId}";
    public TimeSpan? Duration => TimeSpan.FromSeconds(600);
}

