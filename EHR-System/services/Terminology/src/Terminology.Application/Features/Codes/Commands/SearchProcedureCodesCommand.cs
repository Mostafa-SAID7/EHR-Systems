namespace EHRPlatform.Services.Terminology.Application.Features.Codes.Commands;

using MediatR;

/// <summary>
/// Command to search for procedure codes (CPT, HCPCS).
/// Full-text search via Elasticsearch with pricing/RVU data.
/// </summary>
public class SearchProcedureCodesCommand : IRequest<SearchProcedureCodesResponse>
{
    public string SearchTerm { get; set; } = string.Empty;
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? SpecialtyFilter { get; set; }
}

public class SearchProcedureCodesResponse
{
    public List<ProcedureCodeDto> Results { get; set; } = new();
    public int TotalResults { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
}

public class ProcedureCodeDto
{
    public Guid CodeId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Display { get; set; } = string.Empty;
    public string? Definition { get; set; }
    public decimal RVU { get; set; } // Relative Value Unit
    public string? Specialty { get; set; }
    public int UsageCount { get; set; }
    public decimal RelevanceScore { get; set; }
}
