namespace EHRPlatform.Services.Analytics.Application.Features.Analytics.Queries;

using MediatR;
using Microsoft.Extensions.Logging;
using EHRPlatform.Services.Analytics.Domain.Repositories;
using EHRPlatform.BuildingBlocks.Caching;
using EHRPlatform.BuildingBlocks.Security.MultiTenancy;

/// <summary>
/// Handler for GetReportsQuery
/// </summary>
public class GetReportsQueryHandler : IRequestHandler<GetReportsQuery, GetReportsResponse>
{
    private readonly IReportRepository _reportRepository;
    private readonly ICacheService _cacheService;
    private readonly ITenantContext _tenantContext;
    private readonly ILogger<GetReportsQueryHandler> _logger;

    public GetReportsQueryHandler(
        IReportRepository reportRepository,
        ICacheService cacheService,
        ITenantContext tenantContext,
        ILogger<GetReportsQueryHandler> logger)
    {
        _reportRepository = reportRepository;
        _cacheService = cacheService;
        _tenantContext = tenantContext;
        _logger = logger;
    }

    public async Task<GetReportsResponse> Handle(GetReportsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting reports - Page {PageNumber}, Size {PageSize}", 
            request.PageNumber, request.PageSize);

        try
        {
            var tenantId = _tenantContext.TenantId;
            if (tenantId == 0)
            {
                return new GetReportsResponse(
                    Success: false,
                    Message: "Tenant context not available",
                    Reports: new(),
                    TotalCount: 0,
                    PageNumber: request.PageNumber,
                    PageSize: request.PageSize);
            }

            // Check cache
            var cacheKey = $"reports:all:{tenantId}";
            var cachedReports = await _cacheService.GetAsync<List<ReportListItemDto>>(cacheKey);

            if (cachedReports != null && cachedReports.Any())
            {
                _logger.LogInformation("Retrieved reports from cache");
                return new GetReportsResponse(
                    Success: true,
                    Message: "Reports retrieved successfully (from cache)",
                    Reports: cachedReports,
                    TotalCount: cachedReports.Count,
                    PageNumber: request.PageNumber,
                    PageSize: request.PageSize);
            }

            // Get from repository
            var reports = await _reportRepository.GetAllAsync(tenantId);

            var dtos = reports
                .OrderByDescending(r => r.CreatedAt)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(r => new ReportListItemDto(
                    Id: r.Id,
                    Name: r.Name,
                    Description: r.Description,
                    ReportType: r.ReportType,
                    IsScheduled: r.IsScheduled,
                    CreatedAt: r.CreatedAt,
                    UpdatedAt: r.UpdatedAt,
                    ExecutionCount: r.Executions?.Count ?? 0))
                .ToList();

            // Cache for 5 minutes
            await _cacheService.SetAsync(cacheKey, dtos, TimeSpan.FromMinutes(5));

            return new GetReportsResponse(
                Success: true,
                Message: "Reports retrieved successfully",
                Reports: dtos,
                TotalCount: reports.Count(),
                PageNumber: request.PageNumber,
                PageSize: request.PageSize);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting reports");
            return new GetReportsResponse(
                Success: false,
                Message: $"Failed to get reports: {ex.Message}",
                Reports: new(),
                TotalCount: 0,
                PageNumber: request.PageNumber,
                PageSize: request.PageSize);
        }
    }
}
