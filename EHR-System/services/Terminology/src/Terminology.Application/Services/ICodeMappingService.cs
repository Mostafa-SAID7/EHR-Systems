namespace EHRPlatform.Services.Terminology.Application.Services;

using EHRPlatform.Services.Terminology.Application.Features.Codes.Commands;

/// <summary>
/// Interface for code mapping service.
/// Finds mappings between different code systems.
/// </summary>
public interface ICodeMappingService
{
    /// <summary>
    /// Gets mappings from one code system to another.
    /// </summary>
    Task<List<CodeMappingDto>> GetMappingsAsync(
        Guid sourceCodeId,
        string sourceCodeSystem,
        string targetCodeSystem,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Reverse lookup - find source codes by target code.
    /// </summary>
    Task<List<CodeMappingDto>> GetReverseMappingsAsync(
        Guid targetCodeId,
        string sourceCodeSystem,
        string targetCodeSystem,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Find all available mappings for a code across all systems.
    /// </summary>
    Task<Dictionary<string, List<CodeMappingDto>>> GetAllMappingsAsync(
        Guid codeId,
        CancellationToken cancellationToken = default);
}
