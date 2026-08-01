namespace EHRPlatform.Services.Terminology.Infrastructure.Services;

using EHRPlatform.Services.Terminology.Application.Services;
using EHRPlatform.Services.Terminology.Application.Features.Codes.Commands;
using EHRPlatform.Services.Terminology.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

/// <summary>
/// Code validation service implementation.
/// Validates codes against code system rules and compliance requirements.
/// </summary>
public class CodeValidationService : ICodeValidationService
{
    private readonly ITerminologyDbContext _context;
    private readonly ILogger<CodeValidationService> _logger;

    public CodeValidationService(
        ITerminologyDbContext context,
        ILogger<CodeValidationService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<ValidateCodeResponse> ValidateCodeAsync(
        string codeSystem,
        string code,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Validating code '{Code}' in system '{CodeSystem}'", code, codeSystem);

        var cs = await _context.CodeSystems
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Name == codeSystem, cancellationToken);

        if (cs == null)
        {
            return new ValidateCodeResponse
            {
                Code = code,
                CodeSystem = codeSystem,
                IsValid = false,
                ValidationMessages = new List<string> { $"Code system '{codeSystem}' not found" }
            };
        }

        var medicalCode = await _context.MedicalCodes
            .AsNoTracking()
            .FirstOrDefaultAsync(mc => mc.CodeSystemId == cs.Id && mc.Code == code, cancellationToken);

        if (medicalCode == null)
        {
            return new ValidateCodeResponse
            {
                Code = code,
                CodeSystem = codeSystem,
                IsValid = false,
                ValidationMessages = new List<string> { $"Code '{code}' not found in system '{codeSystem}'" }
            };
        }

        var messages = new List<string>();

        if (!medicalCode.IsActive)
        {
            messages.Add("Code is inactive");
        }

        return new ValidateCodeResponse
        {
            Code = medicalCode.Code,
            CodeSystem = codeSystem,
            IsValid = medicalCode.IsActive,
            Display = medicalCode.Display,
            Definition = medicalCode.Definition,
            IsActive = medicalCode.IsActive,
            ValidationMessages = messages
        };
    }

    public async Task<List<ValidateCodeResponse>> ValidateCodesAsync(
        string codeSystem,
        List<string> codes,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Validating {Count} codes in system '{CodeSystem}'", codes.Count, codeSystem);

        var results = new List<ValidateCodeResponse>();

        foreach (var code in codes)
        {
            var result = await ValidateCodeAsync(codeSystem, code, cancellationToken);
            results.Add(result);
        }

        return results;
    }

    public async Task<bool> ValidateCodeCombinationAsync(
        string codeSystem1,
        string code1,
        string codeSystem2,
        string code2,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Validating code combination: {Code1} ({System1}) + {Code2} ({System2})", 
            code1, codeSystem1, code2, codeSystem2);

        // Validate both codes exist
        var validation1 = await ValidateCodeAsync(codeSystem1, code1, cancellationToken);
        var validation2 = await ValidateCodeAsync(codeSystem2, code2, cancellationToken);

        if (!validation1.IsValid || !validation2.IsValid)
        {
            return false;
        }

        // Additional business logic can be added here
        // E.g., check for common incompatibilities

        return true;
    }
}
