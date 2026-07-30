using EHRPlatform.Common.Application.Behaviors;
using EHRPlatform.Common.Application.CQRS;
using EHRPlatform.Services.Billing.Application.Reports.Responses;

namespace EHRPlatform.Services.Billing.Features.Reports.Queries;

/// <summary>
/// Get billing report query.
/// </summary>
public record GetBillingReportQuery : IQuery<BillingReportDto>, ICachedQuery
{
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }

    public string CacheKey => $"report_billing_{StartDate:yyyyMMdd}_{EndDate:yyyyMMdd}";
    public TimeSpan? Duration => TimeSpan.FromSeconds(3600);
}

