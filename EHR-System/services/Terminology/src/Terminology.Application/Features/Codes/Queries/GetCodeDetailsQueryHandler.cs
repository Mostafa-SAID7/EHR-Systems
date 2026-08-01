namespace EHRPlatform.Services.Terminology.Application.Features.Codes.Queries;

using MediatR;
using EHRPlatform.Services.Terminology.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

/// <summary>
/// Handler for GetCodeDetailsQuery - Retrieves full code details.
/// </summary>
public class GetCodeDetailsQueryHandler : IRequestHandler<GetCodeDetailsQuery, CodeDetailsDto>
{
    private readonly ITerminologyDbContext _context;
    private readonly ILogger<GetCodeDetailsQueryHandler> _logger;

    public GetCodeDetailsQueryHandler(
        ITerminologyDbContext context,
        ILogger<GetCodeDetailsQueryHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<CodeDetailsDto> Handle(GetCodeDetailsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting details for code '{Code}' in system '{CodeSystem}'", 
            request.Code, request.CodeSystem);

        var codeSystem = await _context.CodeSystems
            .AsNoTracking()
            .FirstOrDefaultAsync(cs => cs.Name == request.CodeSystem, cancellationToken);

        if (codeSystem == null)
        {
            throw new InvalidOperationException($"Code system '{request.CodeSystem}' not found");
        }

        var medicalCode = await _context.MedicalCodes
            .AsNoTracking()
            .Include(mc => mc.SourceMappings)
            .Include(mc => mc.TargetMappings)
            .FirstOrDefaultAsync(mc => mc.CodeSystemId == codeSystem.Id && mc.Code == request.Code, cancellationToken);

        if (medicalCode == null)
        {
            throw new InvalidOperationException($"Code '{request.Code}' not found in system '{request.CodeSystem}'");
        }

        var relatedCodes = new List<RelatedCodeDto>();
        foreach (var mapping in medicalCode.SourceMappings.Take(5))
        {
            relatedCodes.Add(new RelatedCodeDto
            {
                Code = mapping.TargetCode.Code,
                Display = mapping.TargetCode.Display,
                Relationship = mapping.MappingType
            });
        }

        return new CodeDetailsDto
        {
            Code = medicalCode.Code,
            Display = medicalCode.Display,
            Definition = medicalCode.Definition,
            CodeSystem = request.CodeSystem,
            IsActive = medicalCode.IsActive,
            Category = medicalCode.Category,
            UsageCount = medicalCode.UsageCount,
            RelatedCodes = relatedCodes,
            CreatedAt = medicalCode.CreatedAt,
            UpdatedAt = medicalCode.UpdatedAt
        };
    }
}
