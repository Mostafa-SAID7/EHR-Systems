namespace EHRPlatform.Services.AI.Application.Features.Risk.Queries;

using MediatR;
using EHRPlatform.Services.AI.Persistence;
using Microsoft.Extensions.Logging;
using System.Text.Json;

/// <summary>
/// Handler for GetRiskScoreQuery - Retrieves risk score.
/// </summary>
public class GetRiskScoreQueryHandler : IRequestHandler<GetRiskScoreQuery, RiskScoreDto>
{
    private readonly IAIDbContext _context;
    private readonly ILogger<GetRiskScoreQueryHandler> _logger;

    public GetRiskScoreQueryHandler(
        IAIDbContext context,
        ILogger<GetRiskScoreQueryHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<RiskScoreDto> Handle(GetRiskScoreQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving risk score {RiskScoreId}", request.RiskScoreId);

        var riskScore = await _context.RiskScores.FindAsync(new object[] { request.RiskScoreId }, cancellationToken);
        if (riskScore == null)
        {
            throw new InvalidOperationException($"Risk score {request.RiskScoreId} not found");
        }

        var topFactors = new List<RiskFactorDto>();
        if (!string.IsNullOrEmpty(riskScore.TopRiskFactors))
        {
            try
            {
                topFactors = JsonSerializer.Deserialize<List<RiskFactorDto>>(riskScore.TopRiskFactors) ?? new();
            }
            catch { }
        }

        return new RiskScoreDto
        {
            RiskScoreId = riskScore.Id,
            PatientId = riskScore.PatientId,
            ReadmissionRisk = riskScore.ReadmissionRisk,
            MortalityRisk = riskScore.MortalityRisk,
            InfectionRisk = riskScore.InfectionRisk,
            ChronicDiseaseRisk = riskScore.ChronicDiseaseRisk,
            ComplicationRisk = riskScore.ComplicationRisk,
            OverallRisk = riskScore.OverallRisk,
            RiskLevel = riskScore.RiskLevel,
            TopFactors = topFactors,
            ModelConfidence = riskScore.ModelConfidence,
            CreatedAt = riskScore.CreatedAt
        };
    }
}
