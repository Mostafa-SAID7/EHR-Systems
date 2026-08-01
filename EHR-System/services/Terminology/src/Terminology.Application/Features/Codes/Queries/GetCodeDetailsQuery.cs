namespace EHRPlatform.Services.Terminology.Application.Features.Codes.Queries;

using MediatR;

/// <summary>
/// Query to get detailed information about a specific code.
/// Includes mappings, validation rules, and related codes.
/// </summary>
public class GetCodeDetailsQuery : IRequest<CodeDetailsDto>
{
    public string CodeSystem { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
}

public class CodeDetailsDto
{
    public string Code { get; set; } = string.Empty;
    public string Display { get; set; } = string.Empty;
    public string? Definition { get; set; }
    public string CodeSystem { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public string Category { get; set; } = string.Empty;
    public int UsageCount { get; set; }
    public List<RelatedCodeDto> RelatedCodes { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class RelatedCodeDto
{
    public string Code { get; set; } = string.Empty;
    public string Display { get; set; } = string.Empty;
    public string Relationship { get; set; } = string.Empty; // MAPS_TO, RELATED_TO, etc.
}
