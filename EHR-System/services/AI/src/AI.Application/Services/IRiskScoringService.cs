namespace EHRPlatform.Services.AI.Application.Services;

/// <summary>
/// Interface for risk scoring service.
/// Computes patient risk scores using ML models.
/// </summary>
public interface IRiskScoringService
{
    /// <summary>
    /// Computes risk scores for a patient.
    /// </summary>
    Task<RiskComputationResult> ComputeRiskAsync(
        Guid patientId,
        Dictionary<string, object> features,
        CancellationToken cancellationToken = default);
}

public class RiskComputationResult
{
    public string ModelVersion { get; set; } = string.Empty;
    public decimal ReadmissionRisk { get; set; }
    public decimal MortalityRisk { get; set; }
    public decimal InfectionRisk { get; set; }
    public decimal ChronicDiseaseRisk { get; set; }
    public decimal ComplicationRisk { get; set; }
    public decimal OverallRisk { get; set; }
    public string RiskLevel { get; set; } = string.Empty;
    public List<RiskFactorDto> TopFactors { get; set; } = new();
    public decimal ModelConfidence { get; set; }
    public string? ModelName { get; set; }
}

public class RiskFactorDto
{
    public string FactorName { get; set; } = string.Empty;
    public decimal Importance { get; set; }
    public string? Description { get; set; }
}
