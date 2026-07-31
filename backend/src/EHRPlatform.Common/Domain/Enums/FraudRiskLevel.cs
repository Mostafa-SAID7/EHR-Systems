#nullable enable

namespace EHRPlatform.Common.Domain.Enums;

/// <summary>
/// Fraud risk levels for insurance claim and transaction evaluation.
/// Used to categorize fraud detection results.
/// </summary>
public enum FraudRiskLevel
{
    /// <summary>Low risk - minimal fraud indicators (0-20 score).</summary>
    Low = 0,

    /// <summary>Medium risk - some fraud indicators present (20-50 score).</summary>
    Medium = 1,

    /// <summary>High risk - multiple fraud indicators (50-80 score).</summary>
    High = 2,

    /// <summary>Critical risk - strong fraud indicators (80-100 score).</summary>
    Critical = 3,

    /// <summary>Unable to assess - insufficient data.</summary>
    Unknown = 4
}
