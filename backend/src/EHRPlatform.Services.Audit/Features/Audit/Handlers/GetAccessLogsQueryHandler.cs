using EHRPlatform.BuildingBlocks.Common.Application.CQRS;
using EHRPlatform.BuildingBlocks.Contracts.DTOs;
using EHRPlatform.Services.Audit.Features.Audit.Queries;
using EHRPlatform.Services.Audit.Application.Audit.Responses;
using EHRPlatform.Services.Audit.Application.Audit.Mappers;
using Microsoft.Extensions.Logging;

namespace EHRPlatform.Services.Audit.Features.Audit.Handlers;

/// <summary>
/// Handler for GetAccessLogsQuery.
/// Retrieves paginated access logs with filtering.
/// </summary>
public class GetAccessLogsQueryHandler : IQueryHandler<GetAccessLogsQuery, PagedResult<AccessLogResponse>>
{
    private readonly AuditMapper _mapper;
    private readonly ILogger<GetAccessLogsQueryHandler> _logger;

    public GetAccessLogsQueryHandler(
        AuditMapper mapper,
        ILogger<GetAccessLogsQueryHandler> logger)
    {
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _logger = logger;
    }

    public async Task<PagedResult<AccessLogResponse>> Handle(GetAccessLogsQuery query, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Retrieving access logs with filters - User: {UserId}, Resource: {ResourceType}", 
            query.UserId, query.ResourceType);

        // TODO: Implement repository query with filters
        // This is a stub - implementation would fetch from database
        var accessLogs = new List<Domain.Entities.AccessLog>();
        var total = 0;

        return _mapper.MapToAccessLogPagedResult(accessLogs, total, query.PageNumber, query.PageSize);
    }
}


