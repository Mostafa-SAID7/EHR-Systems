namespace EHRPlatform.Services.AI.Application.Features.Risk.Commands;

using MediatR;
using EHRPlatform.Services.AI.Domain.Entities;
using EHRPlatform.Services.AI.Persistence;
using EHRPlatform.Services.AI.Application.Services;
using Microsoft.Extensions.Logging;

/// <summary>
/// Handler for PredictRiskCommand - Computes patient risk scores.
/// </summary>
public class PredictRiskCommandHandler : IRequestHandler<PredictRiskCommand, PredictRiskResponse>
{
    private readonly IAIDbContext _context;
    private readonly IRiskScoringService _riskScoringService;
    private readonly ILogger<PredictRiskCommandHandler> _logger;

    public PredictRiskCommandHandler(
        IAIDbContext context,
        IRiskScoringService riskScoringService,
        ILogger<PredictRiskCommandHandler> logger)
    {
        _context = context;
        _riskScoringService = riskScoringService;
        _logger = logger;
    }

    public async Task<PredictRiskResponse> Handle(PredictRiskCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Computing risk scores for patient {PatientId}", request.PatientId);

        // Get patient features (from Patient service, cached, or provided)
        var features = request.PatientFeatures ?? await FetchPatientFeaturesAsync(request.PatientId, cancellationToken);

        // Compute risk scores using ML service
        var riskResult = await _riskScoringService.ComputeRiskAsync(request.PatientId, features, cancellationToken);

        // Create risk score entity
        var riskScore = new RiskScore
        {
            Id = Guid.NewGuid(),
            PatientId = request.PatientId,
            EncounterId = request.EncounterId,
            ModelVersion = riskResult.ModelVersion,
            ReadmissionRisk = riskResult.ReadmissionRisk,
            MortalityRisk = riskResult.MortalityRisk,
            InfectionRisk = riskResult.InfectionRisk,
            ChronicDiseaseRisk = riskResult.ChronicDiseaseRisk,
            ComplicationRisk = riskResult.ComplicationRisk,
            OverallRisk = riskResult.OverallRisk,
            RiskLevel = riskResult.RiskLevel,
            TopRiskFactors = System.Text.Json.JsonSerializer.Serialize(riskResult.TopFactors),
            FeatureCount = features.Count,
            ModelConfidence = riskResult.ModelConfidence,
            ModelName = riskResult.ModelName,
            CreatedAt = DateTime.UtcNow
        };

        _context.RiskScores.Add(riskScore);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Risk scores computed for patient {PatientId}. Overall: {OverallRisk:F2}%", 
            request.PatientId, riskScore.OverallRisk);

        return new PredictRiskResponse
        {
            RiskScoreId = riskScore.Id,
            PatientId = riskScore.PatientId,
            ReadmissionRisk = riskScore.ReadmissionRisk,
            MortalityRisk = riskScore.MortalityRisk,
            InfectionRisk = riskScore.InfectionRisk,
            OverallRisk = riskScore.OverallRisk,
            RiskLevel = riskScore.RiskLevel,
            TopRiskFactors = riskResult.TopFactors.Select(f => new RiskFactorDto
            {
                FactorName = f.FactorName,
                Importance = f.Importance,
                Description = f.Description
            }).ToList(),
            ModelConfidence = riskScore.ModelConfidence
        };
    }

    private async Task<Dictionary<string, object>> FetchPatientFeaturesAsync(Guid patientId, CancellationToken cancellationToken)
    {
        // In production: fetch from Patient service or feature store
        return await Task.FromResult(new Dictionary<string, object>
        {
            { "age", 65 },
            { "gender", "M" },
            { "comorbidities", 3 },
            { "previousAdmissions", 2 },
            { "chronicConditions", new[] { "Diabetes", "Hypertension" } }
        });
    }
}
