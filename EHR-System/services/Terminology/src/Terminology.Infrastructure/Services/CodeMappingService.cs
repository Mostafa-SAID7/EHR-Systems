namespace EHRPlatform.Services.Terminology.Infrastructure.Services;

using EHRPlatform.Services.Terminology.Application.Services;
using EHRPlatform.Services.Terminology.Application.Features.Codes.Commands;
using EHRPlatform.Services.Terminology.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

/// <summary>
/// Code mapping service implementation.
/// Provides mappings between different medical code systems.
/// </summary>
public class CodeMappingService : ICodeMappingService
{
    private readonly ITerminologyDbContext _context;
    private readonly ILogger<CodeMappingService> _logger;

    public CodeMappingService(
        ITerminologyDbContext context,
        ILogger<CodeMappingService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<CodeMappingDto>> GetMappingsAsync(
        Guid sourceCodeId,
        string sourceCodeSystem,
        string targetCodeSystem,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting mappings from {SourceSystem} to {TargetSystem} for code {CodeId}", 
            sourceCodeSystem, targetCodeSystem, sourceCodeId);

        var mappings = await _context.CodeMappings
            .AsNoTracking()
            .Where(m => m.SourceCodeId == sourceCodeId)
            .Include(m => m.TargetCode)
            .ThenInclude(c => c.CodeSystem)
            .Where(m => m.TargetCode.CodeSystem.Name == targetCodeSystem)
            .ToListAsync(cancellationToken);

        return mappings.Select(m => new CodeMappingDto
        {
            TargetCodeId = m.TargetCodeId,
            Code = m.TargetCode.Code,
            Display = m.TargetCode.Display,
            MappingType = m.MappingType,
            Confidence = m.Confidence,
            IsApproved = m.IsApproved
        }).ToList();
    }

    public async Task<List<CodeMappingDto>> GetReverseMappingsAsync(
        Guid targetCodeId,
        string sourceCodeSystem,
        string targetCodeSystem,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting reverse mappings for code {CodeId}", targetCodeId);

        var mappings = await _context.CodeMappings
            .AsNoTracking()
            .Where(m => m.TargetCodeId == targetCodeId)
            .Include(m => m.SourceCode)
            .ThenInclude(c => c.CodeSystem)
            .Where(m => m.SourceCode.CodeSystem.Name == sourceCodeSystem)
            .ToListAsync(cancellationToken);

        return mappings.Select(m => new CodeMappingDto
        {
            TargetCodeId = m.SourceCodeId,
            Code = m.SourceCode.Code,
            Display = m.SourceCode.Display,
            MappingType = m.MappingType,
            Confidence = m.Confidence,
            IsApproved = m.IsApproved
        }).ToList();
    }

    public async Task<Dictionary<string, List<CodeMappingDto>>> GetAllMappingsAsync(
        Guid codeId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting all mappings for code {CodeId}", codeId);

        var allMappings = new Dictionary<string, List<CodeMappingDto>>();

        var mappings = await _context.CodeMappings
            .AsNoTracking()
            .Where(m => m.SourceCodeId == codeId)
            .Include(m => m.TargetCode)
            .ThenInclude(c => c.CodeSystem)
            .ToListAsync(cancellationToken);

        foreach (var mapping in mappings)
        {
            var targetSystem = mapping.TargetCode.CodeSystem.Name;
            if (!allMappings.ContainsKey(targetSystem))
            {
                allMappings[targetSystem] = new List<CodeMappingDto>();
            }

            allMappings[targetSystem].Add(new CodeMappingDto
            {
                TargetCodeId = mapping.TargetCodeId,
                Code = mapping.TargetCode.Code,
                Display = mapping.TargetCode.Display,
                MappingType = mapping.MappingType,
                Confidence = mapping.Confidence,
                IsApproved = mapping.IsApproved
            });
        }

        return allMappings;
    }
}
