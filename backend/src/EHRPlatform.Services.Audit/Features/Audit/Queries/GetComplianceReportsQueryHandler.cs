using EHRPlatform.BuildingBlocks.Common.Application.CQRS;
using EHRPlatform.BuildingBlocks.Common.Data.Abstractions;

namespace EHRPlatform.Services.Audit.Features.Audit.Queries;

/// <summary>
/// Get compliance reports handler.
/// Single Responsibility: Retrieve compliance report summaries filtered by a date period.
/// </summary>
public class GetComplianceReportsQueryHandler : IQueryHandler<GetComplianceReportsQuery, List<ComplianceReportDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GetComplianceReportsQueryHandler> _logger;

    public GetComplianceReportsQueryHandler(IUnitOfWork unitOfWork, ILogger<GetComplianceReportsQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<List<ComplianceReportDto>> Handle(
        GetComplianceReportsQuery request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching compliance reports");

        var fromDate = request.FromDate ?? DateTime.UtcNow.AddMonths(-3);
        var toDate = request.ToDate ?? DateTime.UtcNow;

        var repo = _unitOfWork.Repository<ComplianceReport>();
        var reports = await repo.ToListAsync(
            q => q.Where(r => r.PeriodStart >= fromDate && r.PeriodEnd <= toDate)
                .OrderByDescending(r => r.PeriodStart),
            cancellationToken);

        return reports.Select(r => new ComplianceReportDto
        {
            Id = r.Id,
            PeriodStart = r.PeriodStart,
            PeriodEnd = r.PeriodEnd,
            TotalActions = r.TotalActions,
            FailedActions = r.FailedActions,
            DataAccess = r.DataAccess,
            DataChanges = r.DataChanges,
            UnauthorizedAttempts = r.UnauthorizedAttempts,
            PiiAccessed = r.PiiAccessed?.Count ?? 0,
            Status = r.Status,
            GeneratedAt = r.CreatedAt
        }).ToList();
    }
}


