namespace EHRPlatform.Services.Audit.Application.Features.Audit.Queries;

using MediatR;
using EHRPlatform.Services.Audit.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

/// <summary>
/// Handler for GetResourceAuditTrailQuery - Retrieves audit trail for resource.
/// </summary>
public class GetResourceAuditTrailQueryHandler : IRequestHandler<GetResourceAuditTrailQuery, GetResourceAuditTrailResponse>
{
    private readonly IAuditDbContext _context;
    private readonly ILogger<GetResourceAuditTrailQueryHandler> _logger;

    public GetResourceAuditTrailQueryHandler(
        IAuditDbContext context,
        ILogger<GetResourceAuditTrailQueryHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<GetResourceAuditTrailResponse> Handle(GetResourceAuditTrailQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting audit trail for {ResourceType}/{ResourceId}", request.ResourceType, request.ResourceId);

        try
        {
            var query = _context.AuditEntries
                .Where(a => a.ResourceType == request.ResourceType && a.ResourceId == request.ResourceId)
                .OrderByDescending(a => a.CreatedAt);

            var totalCount = await query.CountAsync(cancellationToken);

            var entries = await query
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(a => new AuditEntryDto
                {
                    Id = a.Id,
                    UserId = a.UserId,
                    UserEmail = a.UserEmail,
                    UserFullName = a.UserFullName,
                    Action = a.Action,
                    ResourceType = a.ResourceType,
                    ResourceId = a.ResourceId,
                    Status = a.Status,
                    IpAddress = a.IpAddress,
                    HttpMethod = a.HttpMethod,
                    ChangeDetails = a.ChangeDetails,
                    ContainsSsn = a.ContainsSsn,
                    ContainsDob = a.ContainsDob,
                    ContainsMrn = a.ContainsMrn,
                    CreatedAt = a.CreatedAt
                })
                .ToListAsync(cancellationToken);

            return new GetResourceAuditTrailResponse
            {
                Success = true,
                Entries = entries,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting audit trail");
            return new GetResourceAuditTrailResponse
            {
                Success = false,
                Message = "An error occurred while retrieving the audit trail"
            };
        }
    }
}
