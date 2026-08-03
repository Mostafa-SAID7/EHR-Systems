#nullable enable

namespace Identity.Contracts.Responses;

/// <summary>
/// MFA verification response DTO.
/// </summary>
public class VerifyMfaResponse
{
    /// <summary>
    /// Whether the MFA code was valid.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Message describing the result.
    /// </summary>
    public string Message { get; set; } = string.Empty;
}

