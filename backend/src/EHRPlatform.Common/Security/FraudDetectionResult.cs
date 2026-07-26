namespace EHRPlatform.Common.Security;

/// <summary>
/// Fraud risk levels for claim evaluation.
/// </summary>
public enum FraudRiskLevel
{
    Low = 0,
    Medium = 1,
    High = 2,
    Critical = 3
}

/// <summary>
/// Result of a fraud risk assessment on an insurance claim or transaction.
/// </summary>
public record FraudDetectionResult
{
    /// <summary>Fraud score from 0 (completely safe) to 100 (definite fraud).</summary>
    public decimal RiskScore { get; init; }

    /// <summary>Categorized risk level.</summary>
    public FraudRiskLevel RiskLevel { get; init; }

    /// <summary>List of suspicious flags triggered during evaluation.</summary>
    public IReadOnlyList<string> Flags { get; init; } = Array.Empty<string>();

    /// <summary>Timestamp of evaluation.</summary>
    public DateTime EvaluatedAt { get; init; } = DateTime.UtcNow;

    /// <summary>Recommendation on claim handling (AutoApprove, ManualReview, Reject).</summary>
    public string Recommendation { get; init; } = "AutoApprove";

    public bool IsHighRisk => RiskScore >= 80m || RiskLevel >= FraudRiskLevel.High;
}
