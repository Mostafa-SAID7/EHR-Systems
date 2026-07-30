using EHRPlatform.Common.Infrastructure.Security;

namespace EHRPlatform.Services.Billing.Infrastructure.Services;

/// <summary>
/// Default implementation of IFraudDetectionService using rule-based scoring.
/// Analyzes total claim amount, procedure code risk, and unbundling patterns.
/// </summary>
public class FraudDetectionService : IFraudDetectionService
{
    public Task<FraudDetectionResult> EvaluateClaimAsync(
        Guid claimId,
        decimal amount,
        string provider,
        IEnumerable<string> procedureCodes,
        CancellationToken cancellationToken = default)
    {
        var flags = new List<string>();
        decimal riskScore = 10m; // Base low risk score

        var codeList = procedureCodes.ToList();

        // Rule 1: High claim amount threshold
        if (amount > 10000m)
        {
            riskScore += 35m;
            flags.Add("High claim amount threshold exceeded (> $10,000)");
        }

        // Rule 2: Multiple high-cost surgical/procedure codes billed together
        if (codeList.Count > 5)
        {
            riskScore += 25m;
            flags.Add("Excessive line items / potential unbundling pattern");
        }

        // Rule 3: Missing provider NPI or generic provider name
        if (string.IsNullOrWhiteSpace(provider) || provider.Equals("Unknown", StringComparison.OrdinalIgnoreCase))
        {
            riskScore += 20m;
            flags.Add("Unverified provider identifier");
        }

        var riskLevel = riskScore switch
        {
            >= 80m => FraudRiskLevel.Critical,
            >= 60m => FraudRiskLevel.High,
            >= 30m => FraudRiskLevel.Medium,
            _ => FraudRiskLevel.Low
        };

        var recommendation = riskLevel >= FraudRiskLevel.High ? "ManualReview" : "AutoApprove";

        return Task.FromResult(new FraudDetectionResult
        {
            RiskScore = riskScore,
            RiskLevel = riskLevel,
            Flags = flags.AsReadOnly(),
            EvaluatedAt = DateTime.UtcNow,
            Recommendation = recommendation
        });
    }
}

