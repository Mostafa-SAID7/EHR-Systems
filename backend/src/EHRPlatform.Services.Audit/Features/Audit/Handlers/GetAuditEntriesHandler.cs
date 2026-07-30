using MediatR;
using EHRPlatform.Services.Audit.Features.Audit.Queries;
using EHRPlatform.Services.Audit.Application.Audit.Responses;
using EHRPlatform.Common.Data;
using EHRPlatform.Common.Shared.DTOs;
using Microsoft.Extensions.Logging;

namespace EHRPlatform.Services.Audit.Features.Audit.Handlers;

/// <summary>
/// Handler for GetAuditEntriesQuery.
/// </summary>
public class GetAuditEntriesHandler : IRequestHandler<GetAuditEntriesQuery, PagedResult<AuditEntryResponse>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GetAuditEntriesHandler> _logger;

    public GetAuditEntriesHandler(IUnitOfWork unitOfWork, ILogger<GetAuditEntriesHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<PagedResult<AuditEntryResponse>> Handle(GetAuditEntriesQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching audit entries page {Page}", request.PageNumber);

        var repo = _unitOfWork.Repository<Domain.Entities.AuditEntry>();
        var skip = (request.PageNumber - 1) * request.PageSize;

        var total = await repo.CountAsync(
            q => q.Where(e =>
                (request.UserId == null || e.UserId == request.UserId) &&
                (request.ResourceType == null || e.ResourceType == request.ResourceType) &&
                (request.Action == null || e.Action == request.Action) &&
                (request.StartDate == null || e.Timestamp >= request.StartDate) &&
                (request.EndDate == null || e.Timestamp <= request.EndDate)),
            cancellationToken);

        var entries = await repo.ToListAsync(
            q => q.Where(e =>
                (request.UserId == null || e.UserId == request.UserId) &&
                (request.ResourceType == null || e.ResourceType == request.ResourceType) &&
                (request.Action == null || e.Action == request.Action) &&
                (request.StartDate == null || e.Timestamp >= request.StartDate) &&
                (request.EndDate == null || e.Timestamp <= request.EndDate))
                .OrderByDescending(e => e.Timestamp)
                .Skip(skip)
                .Take(request.PageSize),
            cancellationToken);

        var items = entries.Select(e => new AuditEntryResponse
        {
            Id = e.Id,
            UserId = e.UserId,
            UserEmail = e.UserEmail,
            Action = e.Action,
            ResourceType = e.ResourceType,
            ResourceId = e.ResourceId.ToString(),
            Status = e.Status,
            Timestamp = e.Timestamp,
            Details = e.FailureReason
        }).ToList();

        return PagedResult<AuditEntryResponse>.Create(items, total, request.PageNumber, request.PageSize);
    }
}

