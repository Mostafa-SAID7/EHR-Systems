namespace EHRPlatform.Services.Terminology.Application.Features.Codes.Commands;

using MediatR;

/// <summary>
/// Command to validate a code against a code system.
/// Checks existence, activity status, and compliance rules.
/// </summary>
public class ValidateCodeCommand : IRequest<ValidateCodeResponse>
{
    public string CodeSystem { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
}

public class ValidateCodeResponse
{
    public string Code { get; set; } = string.Empty;
    public string CodeSystem { get; set; } = string.Empty;
    public bool IsValid { get; set; }
    public string? Display { get; set; }
    public string? Definition { get; set; }
    public bool IsActive { get; set; }
    public List<string> ValidationMessages { get; set; } = new();
}
