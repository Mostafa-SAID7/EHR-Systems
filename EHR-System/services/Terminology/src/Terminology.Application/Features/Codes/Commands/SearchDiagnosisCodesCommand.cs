namespace EHRPlatform.Services.Terminology.Application.Features.Codes.Commands;

using MediatR;

/// <summary>
/// Command to search for diagnosis codes (ICD-10).
/// Full-text search via Elasticsearch with filtering and pagination.
/// </summary>
public class SearchDiagnosisCodesCommand : IRequest<SearchDiagnosisCodesResponse>
{
    public string SearchTerm { get; set; } = string.Empty;
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? Category { get; set; } // Filter by category
}

public class SearchDiagnosisCodesResponse
{
    public List<DiagnosisCodeDto> Results { get; set; } = new();
    public int TotalResults { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
}

public class DiagnosisCodeDto
{
    public Guid CodeId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Display { get; set; } = string.Empty;
    public string? Definition { get; set; }
    public string Category { get; set; } = string.Empty;
    public int UsageCount { get; set; }
    public decimal RelevanceScore { get; set; }
}
