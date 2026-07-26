namespace EHRPlatform.Common.Security;

/// <summary>
/// Interface for claim fraud detection and anomaly scoring.
/// Evaluates claim parameters (unusual procedure combinations, duplicate billing, billing pattern anomalies).
/// </summary>
public interface IFraudDetectionService
{
    /// <summary>
    /// Evaluates an insurance claim for potential fraud or unbundling.
    /// </summary>
    Task<FraudDetectionResult> EvaluateClaimAsync(
        Guid claimId,
        decimal amount,
        string provider,
        IEnumerable<string> procedureCodes,
        CancellationToken cancellationToken = default);
}
