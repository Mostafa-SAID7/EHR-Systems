namespace EHRPlatform.Services.Identity.Application.Features.Auth.Commands;

using MediatR;

/// <summary>
/// Command to register new user account.
/// Generates temporary password, sends email verification.
/// </summary>
public class RegisterCommand : IRequest<RegisterResponse>
{
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
}

public class RegisterResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public Guid? UserId { get; set; }
    public string? VerificationEmailSent { get; set; }
}
