#nullable enable

namespace EHRPlatform.Services.Identity.Contracts.Responses;

/// <summary>
/// Change password response DTO.
/// </summary>
public class ChangePasswordResponse
{
    /// <summary>
    /// Success message.
    /// </summary>
    public string Message { get; set; } = "Password changed successfully";

    /// <summary>
    /// Timestamp when password was changed.
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

