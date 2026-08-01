namespace EHRPlatform.Services.Terminology.Application.Features.Codes.Queries;

using MediatR;

/// <summary>
/// Query for code autocomplete/suggestions.
/// Returns best matches as user types.
/// </summary>
public class AutocompleteCodesQuery : IRequest<AutocompleteCodesResponse>
{
    public string CodeSystem { get; set; } = string.Empty;
    public string Prefix { get; set; } = string.Empty;
    public int MaxResults { get; set; } = 10;
}

public class AutocompleteCodesResponse
{
    public List<AutocompleteCodeDto> Suggestions { get; set; } = new();
}

public class AutocompleteCodeDto
{
    public string Code { get; set; } = string.Empty;
    public string Display { get; set; } = string.Empty;
    public int UsageCount { get; set; }
}
