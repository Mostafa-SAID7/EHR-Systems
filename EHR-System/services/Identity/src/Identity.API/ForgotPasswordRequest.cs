#nullable enable

namespace EHRPlatform.Services.Identity.API.Controllers;

/// <summary>
/// Request body for forgot-password endpoint.
/// </summary>
public class ForgotPasswordRequest
{
    /// <summary>
    /// Email address of the account requesting a reset link.
    /// </summary>
    public string Email { get; set; } = string.Empty;
}

