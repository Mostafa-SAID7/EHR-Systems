namespace EHRPlatform.Services.AI.Application.Features.Risk.Queries;

using MediatR;

/// <summary>
/// Query to retrieve risk scores for a patient.
/// </summary>
public class GetRiskScoreQuery : IRequest<RiskScoreDto>
{
    public Guid RiskScoreId { get; set; }
}

public class RiskScoreDto
{
    public Guid RiskScoreId { get; set; }
    public Guid PatientId { get; set; }
    public decimal ReadmissionRisk { get; set; }
    public decimal MortalityRisk { get; set; }
    public decimal InfectionRisk { get; set; }
    public decimal ChronicDiseaseRisk { get; set; }
    public decimal ComplicationRisk { get; set; }
    public decimal OverallRisk { get; set; }
    public string RiskLevel { get; set; } = string.Empty;
    public List<RiskFactorDto> TopFactors { get; set; } = new();
    public decimal ModelConfidence { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class RiskFactorDto
{
    public string FactorName { get; set; } = string.Empty;
    public decimal Importance { get; set; }
    public string? Description { get; set; }
}
