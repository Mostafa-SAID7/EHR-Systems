namespace EHRPlatform.Services.Audit.Application.Features.Audit.Queries;

using MediatR;
using EHRPlatform.Services.Audit.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

/// <summary>
/// Handler for GetUserAuditActivityQuery - Retrieves audit activity for user.
/// </summary>
public class GetUserAuditActivityQueryHandler : IRequestHandler<GetUserAuditActivityQuery, GetUserAuditActivityResponse>
{
    private readonly IAuditDbContext _context;
    private readonly ILogger<GetUserAuditActivityQueryHandler> _logger;

    public GetUserAuditActivityQueryHandler(
        IAuditDbContext context,
        ILogger<GetUserAuditActivityQueryHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<GetUserAuditActivityResponse> Handle(GetUserAuditActivityQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting audit activity for user {UserId}", request.UserId);

        try
        {
            var query = _context.AuditEntries
                .Where(a => a.UserId == request.UserId);

            if (request.FromDate.HasValue)
                query = query.Where(a => a.CreatedAt >= request.FromDate);

            if (request.ToDate.HasValue)
                query = query.Where(a => a.CreatedAt <= request.ToDate);

            var totalCount = await query.CountAsync(cancellationToken);

            var entries = await query
                .OrderByDescending(a => a.CreatedAt)
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
                    CreatedAt = a.CreatedAt
                })
                .ToListAsync(cancellationToken);

            return new GetUserAuditActivityResponse
            {
                Success = true,
                Entries = entries,
                TotalCount = totalCount
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user audit activity");
            return new GetUserAuditActivityResponse
            {
                Success = false,
                Message = "An error occurred while retrieving user activity"
            };
        }
    }
}
