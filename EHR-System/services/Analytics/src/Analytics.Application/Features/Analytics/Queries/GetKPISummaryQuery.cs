namespace EHRPlatform.Services.Analytics.Application.Features.Analytics.Queries;

using MediatR;
using EHRPlatform.Services.Analytics.Contracts.Responses;

/// <summary>
/// Query to get KPI summary (cached 15 minutes).
/// </summary>
public class GetKPISummaryQuery : IRequest<GetKPISummaryResponse>
{
    public DateTime? ForDate { get; set; }
}

