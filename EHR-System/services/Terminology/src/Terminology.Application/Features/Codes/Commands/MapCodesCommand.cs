namespace EHRPlatform.Services.Terminology.Application.Features.Codes.Commands;

using MediatR;

/// <summary>
/// Command to find code mappings between different code systems.
/// Maps ICD-10 to SNOMED CT, CPT to RxNorm, etc.
/// </summary>
public class MapCodesCommand : IRequest<MapCodesResponse>
{
    public Guid SourceCodeId { get; set; }
    public string SourceCodeSystem { get; set; } = string.Empty; // ICD-10, CPT, etc.
    public string TargetCodeSystem { get; set; } = string.Empty; // SNOMED CT, RxNorm, etc.
}

public class MapCodesResponse
{
    public Guid SourceCodeId { get; set; }
    public string SourceCodeSystem { get; set; } = string.Empty;
    public string TargetCodeSystem { get; set; } = string.Empty;
    public List<CodeMappingDto> Mappings { get; set; } = new();
}

public class CodeMappingDto
{
    public Guid TargetCodeId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Display { get; set; } = string.Empty;
    public string MappingType { get; set; } = string.Empty; // EXACT_MATCH, NARROWER, BROADER
    public decimal Confidence { get; set; }
    public bool IsApproved { get; set; }
}
