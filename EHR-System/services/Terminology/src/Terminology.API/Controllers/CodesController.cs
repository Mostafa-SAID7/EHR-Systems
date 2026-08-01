namespace EHRPlatform.Services.Terminology.API.Controllers;

using MediatR;
using EHRPlatform.Services.Terminology.Application.Features.Codes.Commands;
using EHRPlatform.Services.Terminology.Application.Features.Codes.Queries;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.ComponentModel.DataAnnotations;

/// <summary>
/// API endpoints for medical code operations.
/// Search, map, and validate medical codes across ICD-10, CPT, RxNorm, LOINC systems.
/// </summary>
[ApiController]
[Route("api/v1/codes")]
[Authorize]
public class CodesController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<CodesController> _logger;

    public CodesController(IMediator mediator, ILogger<CodesController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Search for diagnosis codes (ICD-10).
    /// GET /api/v1/codes/diagnoses/search?term=diabetes&page=1&pageSize=20
    /// </summary>
    [HttpGet("diagnoses/search")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> SearchDiagnosis(
        [FromQuery] string term,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Searching diagnosis codes for '{Term}'", term);

        var command = new SearchDiagnosisCodesCommand
        {
            SearchTerm = term,
            PageNumber = page,
            PageSize = pageSize
        };

        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Search for procedure codes (CPT).
    /// GET /api/v1/codes/procedures/search?term=office%20visit
    /// </summary>
    [HttpGet("procedures/search")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> SearchProcedure(
        [FromQuery] string term,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Searching procedure codes for '{Term}'", term);

        var command = new SearchProcedureCodesCommand
        {
            SearchTerm = term,
            PageNumber = page,
            PageSize = pageSize
        };

        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Autocomplete code suggestions.
    /// GET /api/v1/codes/autocomplete?system=ICD-10&prefix=E11
    /// </summary>
    [HttpGet("autocomplete")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Autocomplete(
        [FromQuery] string system,
        [FromQuery] string prefix,
        [FromQuery] int maxResults = 10,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Autocompleting codes for '{System}' with prefix '{Prefix}'", system, prefix);

        var query = new AutocompleteCodesQuery
        {
            CodeSystem = system,
            Prefix = prefix,
            MaxResults = maxResults
        };

        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Get detailed information about a code.
    /// GET /api/v1/codes/details?system=ICD-10&code=E11.9
    /// </summary>
    [HttpGet("details")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDetails(
        [FromQuery] string system,
        [FromQuery] string code,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting details for code '{Code}' in system '{System}'", code, system);

        var query = new GetCodeDetailsQuery
        {
            CodeSystem = system,
            Code = code
        };

        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Map code to another code system.
    /// POST /api/v1/codes/map
    /// </summary>
    [HttpPost("map")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MapCode(
        [FromBody] MapCodeRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Mapping code from {SourceSystem} to {TargetSystem}", 
            request.SourceCodeSystem, request.TargetCodeSystem);

        var command = new MapCodesCommand
        {
            SourceCodeId = request.SourceCodeId,
            SourceCodeSystem = request.SourceCodeSystem,
            TargetCodeSystem = request.TargetCodeSystem
        };

        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Validate a code in a code system.
    /// POST /api/v1/codes/validate
    /// </summary>
    [HttpPost("validate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ValidateCode(
        [FromBody] ValidateCodeRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Validating code '{Code}' in system '{System}'", request.Code, request.CodeSystem);

        var command = new ValidateCodeCommand
        {
            CodeSystem = request.CodeSystem,
            Code = request.Code
        };

        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }
}

// Request DTOs
public class MapCodeRequest
{
    [Required]
    public Guid SourceCodeId { get; set; }
    [Required]
    public string SourceCodeSystem { get; set; } = string.Empty;
    [Required]
    public string TargetCodeSystem { get; set; } = string.Empty;
}

public class ValidateCodeRequest
{
    [Required]
    public string CodeSystem { get; set; } = string.Empty;
    [Required]
    public string Code { get; set; } = string.Empty;
}
