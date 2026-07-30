using EHRPlatform.Common.Application.CQRS;
using EHRPlatform.Common.Data;

namespace EHRPlatform.Services.Audit.Features.Audit.Queries;

/// <summary>
/// Get user audit activity handler.
/// Single Responsibility: Aggregate and summarize a user's audit actions for a given date range.
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

