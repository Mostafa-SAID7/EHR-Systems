using EHRPlatform.Common.Application.CQRS;
using EHRPlatform.Common.Data;
using Mapster;

namespace EHRPlatform.Services.Audit.Features.Audit.Queries;

/// <summary>
/// Get resource audit trail handler.
/// Single Responsibility: Retrieve paginated audit entries for a specific resource.
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

