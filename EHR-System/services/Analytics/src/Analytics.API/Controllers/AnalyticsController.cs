namespace EHRPlatform.Services.Analytics.API.Controllers;

using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using EHRPlatform.Services.Analytics.Application.Features.Analytics.Commands;
using EHRPlatform.Services.Analytics.Application.Features.Analytics.Queries;

/// <summary>
/// Analytics API endpoints
/// </summary>
[ApiController]
[Route("api/v1/analytics")]
[Authorize]
public class AnalyticsController : ControllerBase
{
    private readonly IMediator _mediator;

    public AnalyticsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get KPI summary (cached 15 minutes)
    /// </summary>
    [HttpGet("kpi")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetKPISummary([FromQuery] DateTime? forDate)
    {
        var result = await _mediator.Send(new GetKPISummaryQuery { ForDate = forDate });
        return Ok(result);
    }

    /// <summary>
    /// Create new dashboard
    /// </summary>
    [HttpPost("dashboards")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateDashboard([FromBody] CreateDashboardCommand command)
    {
        var result = await _mediator.Send(command);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Health check endpoint
    /// </summary>
    [HttpGet("health")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Health()
    {
        return Ok(new { status = "healthy", service = "AnalyticsService", timestamp = DateTime.UtcNow });
    }
}
