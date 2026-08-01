namespace EHRPlatform.Services.AI.Application.Features.Risk.Commands;

using MediatR;

/// <summary>
/// Command to compute risk scores for a patient.
/// Uses ML models to predict readmission, mortality, infection risks.
/// </summary>
public class PredictRiskCommand : IRequest<PredictRiskResponse>
{
    public Guid PatientId { get; set; }
    public Guid? EncounterId { get; set; }
    public Dictionary<string, object>? PatientFeatures { get; set; }
}

public class PredictRiskResponse
{
    public Guid RiskScoreId { get; set; }
    public Guid PatientId { get; set; }
    public decimal ReadmissionRisk { get; set; }
    public decimal MortalityRisk { get; set; }
    public decimal InfectionRisk { get; set; }
    public decimal OverallRisk { get; set; }
    public string RiskLevel { get; set; } = string.Empty;
    public List<RiskFactorDto> TopRiskFactors { get; set; } = new();
    public decimal ModelConfidence { get; set; }
}

public class RiskFactorDto
{
    public string FactorName { get; set; } = string.Empty;
    public decimal Importance { get; set; }
    public string? Description { get; set; }
}
