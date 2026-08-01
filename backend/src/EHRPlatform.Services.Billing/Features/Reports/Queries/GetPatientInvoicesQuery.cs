using EHRPlatform.BuildingBlocks.EventBus.Behaviors;
using EHRPlatform.BuildingBlocks.EventBus.CQRS;
using EHRPlatform.Services.Billing.Application.Invoicing.Responses;
using EHRPlatform.Services.Billing.Application.Reports.Responses;

namespace EHRPlatform.Services.Billing.Features.Reports.Queries;

/// <summary>
/// Get patient invoices - CACHED query.
/// </summary>
public record GetPatientInvoicesQuery : IQuery<InvoiceListDto>, ICachedQuery
{
    public Guid PatientId { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 50;

    public string CacheKey => $"invoices_patient_{PatientId}_{PageNumber}_{PageSize}";
    public TimeSpan? Duration => TimeSpan.FromSeconds(600);
}



