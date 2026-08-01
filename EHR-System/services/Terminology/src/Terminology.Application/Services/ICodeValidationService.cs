namespace EHRPlatform.Services.Terminology.Application.Services;

using EHRPlatform.Services.Terminology.Application.Features.Codes.Commands;

/// <summary>
/// Interface for code validation service.
/// Validates codes against code system rules and compliance.
/// </summary>
public interface ICodeValidationService
{
    /// <summary>
    /// Validates a code exists and is active in a code system.
    /// </summary>
    Task<ValidateCodeResponse> ValidateCodeAsync(
        string codeSystem,
        string code,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates a batch of codes (for bulk operations).
    /// </summary>
    Task<List<ValidateCodeResponse>> ValidateCodesAsync(
        string codeSystem,
        List<string> codes,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if code combination is valid for a specific context.
    /// E.g., diagnosis + procedure combination for billing.
    /// </summary>
    Task<bool> ValidateCodeCombinationAsync(
        string codeSystem1,
        string code1,
        string codeSystem2,
        string code2,
        CancellationToken cancellationToken = default);
}
