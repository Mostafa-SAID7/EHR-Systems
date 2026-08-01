namespace EHRPlatform.Services.Patient.Application.Services;

/// <summary>
/// Service for generating unique Medical Record Numbers (MRN).
/// Format: MRN-YYYY-XXXXXX (e.g., MRN-2025-000001)
/// </summary>
public interface IMrnGenerationService
{
    /// <summary>
    /// Generate a unique MRN.
    /// </summary>
    Task<string> GenerateMrnAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Validate MRN format.
    /// </summary>
    bool IsValidMrn(string mrn);

    /// <summary>
    /// Parse MRN to get year and sequence.
    /// </summary>
    (int Year, int Sequence) ParseMrn(string mrn);
}
