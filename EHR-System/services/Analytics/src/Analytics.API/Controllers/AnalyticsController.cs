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
    /// Update dashboard configuration
    /// </summary>
    [HttpPut("dashboards/{dashboardId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateDashboard(
        Guid dashboardId,
        [FromBody] UpdateDashboardRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(
            new UpdateDashboardCommand(dashboardId, request.Name, request.Description, request.Configuration),
            cancellationToken);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Delete dashboard
    /// </summary>
    [HttpDelete("dashboards/{dashboardId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteDashboard(
        Guid dashboardId,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new DeleteDashboardCommand(dashboardId), cancellationToken);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Get metrics for date range with optional filters
    /// </summary>
    [HttpGet("metrics")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMetrics(
        [FromQuery] DateTime fromDate,
        [FromQuery] DateTime toDate,
        [FromQuery] string? metricType = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(
            new GetMetricsQuery(fromDate, toDate, metricType, pageNumber, pageSize),
            cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Export analytics data to file format
    /// </summary>
    [HttpPost("export")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ExportData(
        [FromBody] ExportDataRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(
            new ExportDataCommand(request.FromDate, request.ToDate, request.Format, request.Filters),
            cancellationToken);
        
        if (!result.Success)
            return BadRequest(result);
        
        return File(result.FileContent, "application/octet-stream", result.FileName);
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
