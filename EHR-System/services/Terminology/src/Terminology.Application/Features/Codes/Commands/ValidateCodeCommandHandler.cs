namespace EHRPlatform.Services.Terminology.Application.Features.Codes.Commands;

using MediatR;
using EHRPlatform.Services.Terminology.Application.Services;
using Microsoft.Extensions.Logging;

/// <summary>
/// Handler for ValidateCodeCommand - Validates code in code system.
/// </summary>
public class ValidateCodeCommandHandler : IRequestHandler<ValidateCodeCommand, ValidateCodeResponse>
{
    private readonly ICodeValidationService _validationService;
    private readonly ILogger<ValidateCodeCommandHandler> _logger;

    public ValidateCodeCommandHandler(
        ICodeValidationService validationService,
        ILogger<ValidateCodeCommandHandler> logger)
    {
        _validationService = validationService;
        _logger = logger;
    }

    public async Task<ValidateCodeResponse> Handle(ValidateCodeCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Validating code '{Code}' in system '{CodeSystem}'", 
            request.Code, request.CodeSystem);

        var validationResult = await _validationService.ValidateCodeAsync(
            request.CodeSystem,
            request.Code,
            cancellationToken);

        return validationResult;
    }
}
