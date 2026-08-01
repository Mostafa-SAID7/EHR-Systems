namespace EHRPlatform.Services.AI.Application.Features.Anomaly.Queries;

using MediatR;
using EHRPlatform.Services.AI.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

/// <summary>
/// Handler for GetAnomaliesQuery - Retrieves anomalies.
/// </summary>
public class GetAnomaliesQueryHandler : IRequestHandler<GetAnomaliesQuery, GetAnomaliesResponse>
{
    private readonly IAIDbContext _context;
    private readonly ILogger<GetAnomaliesQueryHandler> _logger;

    public GetAnomaliesQueryHandler(
        IAIDbContext context,
        ILogger<GetAnomaliesQueryHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<GetAnomaliesResponse> Handle(GetAnomaliesQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving anomalies. RiskScoreId: {RiskScoreId}, PatientId: {PatientId}", 
            request.RiskScoreId, request.PatientId);

        var query = _context.AnomalyDetections.AsQueryable();

        if (request.RiskScoreId.HasValue)
        {
            query = query.Where(a => a.RiskScoreId == request.RiskScoreId);
        }

        if (request.PatientId.HasValue)
        {
            query = query.Where(a => a.PatientId == request.PatientId);
        }

        if (!string.IsNullOrEmpty(request.AnomalyType))
        {
            query = query.Where(a => a.AnomalyType == request.AnomalyType);
        }

        if (request.OnlyAlerted)
        {
            query = query.Where(a => a.IsAlerted);
        }

        var anomalies = await query
            .OrderByDescending(a => a.DetectedAt)
            .ToListAsync(cancellationToken);

        var alertedCount = anomalies.Count(a => a.IsAlerted);

        return new GetAnomaliesResponse
        {
            Anomalies = anomalies.Select(a => new AnomalyDto
            {
                AnomalyId = a.Id,
                AnomalyType = a.AnomalyType,
                Description = a.Description,
                AnomalyScore = a.AnomalyScore,
                IsAlerted = a.IsAlerted,
                DetectedAt = a.DetectedAt
            }).ToList(),
            TotalCount = anomalies.Count,
            AlertedCount = alertedCount
        };
    }
}
