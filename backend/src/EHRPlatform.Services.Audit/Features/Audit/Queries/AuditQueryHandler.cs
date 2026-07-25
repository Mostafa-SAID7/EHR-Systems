using EHRPlatform.Common.CQRS;
using EHRPlatform.Common.Data;
using Mapster;

namespace EHRPlatform.Services.Audit.Features.Audit.Queries;

/// <summary>
/// Get resource audit trail handler.
/// </summary>
public class GetResourceAuditTrailQueryHandler : IQueryHandler<GetResourceAuditTrailQuery, AuditTrailResponseDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GetResourceAuditTrailQueryHandler> _logger;

    public GetResourceAuditTrailQueryHandler(IUnitOfWork unitOfWork, ILogger<GetResourceAuditTrailQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<AuditTrailResponseDto> Handle(
        GetResourceAuditTrailQuery request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching audit trail for {ResourceType}/{ResourceId}",
            request.ResourceType, request.ResourceId);

        var repo = _unitOfWork.Repository<AuditEntry>();
        var skip = (request.PageNumber - 1) * request.PageSize;

        var total = await repo.CountAsync(
            q => q.Where(e => e.ResourceType == request.ResourceType && e.ResourceId == request.ResourceId),
            cancellationToken);

        var entries = await repo.ToListAsync(
            q => q.Where(e => e.ResourceType == request.ResourceType && e.ResourceId == request.ResourceId)
                .OrderByDescending(e => e.Timestamp)
                .Skip(skip)
                .Take(request.PageSize),
            cancellationToken);

        return new AuditTrailResponseDto
        {
            ResourceType = request.ResourceType,
            ResourceId = request.ResourceId,
            Entries = entries.Select(e => new AuditEntryResponseDto
            {
                Id = e.Id,
                UserId = e.UserId,
                UserEmail = e.UserEmail,
                Action = e.Action,
                Status = e.Status,
                Timestamp = e.Timestamp,
                PiiIndicators = e.PiiIndicators,
                AccessLevel = e.AccessLevel,
                ChangeDetails = e.ChangeDetails,
                FailureReason = e.FailureReason
            }).ToList(),
            Total = total,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }
}

/// <summary>
/// Get user audit activity handler.
/// </summary>
public class GetUserAuditActivityQueryHandler : IQueryHandler<GetUserAuditActivityQuery, AccessLogDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GetUserAuditActivityQueryHandler> _logger;

    public GetUserAuditActivityQueryHandler(IUnitOfWork unitOfWork, ILogger<GetUserAuditActivityQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<AccessLogDto> Handle(
        GetUserAuditActivityQuery request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching audit activity for user {UserId}", request.UserId);

        var repo = _unitOfWork.Repository<AuditEntry>();
        var fromDate = request.FromDate ?? DateTime.UtcNow.AddDays(-30);
        var toDate = request.ToDate ?? DateTime.UtcNow;

        var entries = await repo.ToListAsync(
            q => q.Where(e => e.UserId == request.UserId && e.Timestamp >= fromDate && e.Timestamp <= toDate),
            cancellationToken);

        var userEmail = entries.FirstOrDefault()?.UserEmail ?? "";
        var activities = entries
            .GroupBy(e => e.Action)
            .Select(g => new ActivitySummaryDto
            {
                Action = g.Key,
                Count = g.Count(),
                LastOccurred = g.Max(e => e.Timestamp)
            })
            .ToList();

        return new AccessLogDto
        {
            UserId = request.UserId,
            UserEmail = userEmail,
            Activities = activities,
            TotalActions = entries.Count,
            FailedActions = entries.Count(e => e.Status == "Failure")
        };
    }
}

/// <summary>
/// Get compliance reports handler.
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
            PiiAccessed = r.PiiAccessed,
            Status = r.Status,
            GeneratedAt = r.CreatedAt
        }).ToList();
    }
}
