using Mapster;
using EHRPlatform.Common.Application.Common.Mapping;
using EHRPlatform.Common.Shared.DTOs;
using EHRPlatform.Services.Analytics.Domain.Entities;
using EHRPlatform.Services.Analytics.Application.Analytics.Responses;
using Microsoft.Extensions.Logging;

namespace EHRPlatform.Services.Analytics.Application.Analytics.Mappers;

/// <summary>
/// Analytics Mapper
/// Single Responsibility: Convert between Analytics domain models and DTOs.
/// </summary>
public class AnalyticsMapper : MappingServiceBase<Dashboard, DashboardResponse>
{
    public AnalyticsMapper(ILogger<AnalyticsMapper> logger) : base(logger)
    {
    }

    public PagedResult<DashboardResponse> MapToDashboardPagedResult(
        ICollection<Dashboard> dashboards,
        int total,
        int pageNumber,
        int pageSize)
    {
        Logger.LogDebug("Mapping {Count} dashboards to paged result", dashboards.Count);

        return PagedResult<DashboardResponse>.Create(
            dashboards.Adapt<List<DashboardResponse>>(),
            total,
            pageNumber,
            pageSize);
    }

    public PagedResult<ReportResponse> MapToReportPagedResult(
        ICollection<Report> reports,
        int total,
        int pageNumber,
        int pageSize)
    {
        Logger.LogDebug("Mapping {Count} reports to paged result", reports.Count);

        return PagedResult<ReportResponse>.Create(
            reports.Adapt<List<ReportResponse>>(),
            total,
            pageNumber,
            pageSize);
    }
}

