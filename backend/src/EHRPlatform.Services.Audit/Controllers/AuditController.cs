using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EHRPlatform.Services.Audit.Features.Audit.Commands;
using EHRPlatform.Services.Audit.Features.Audit.Queries;

namespace EHRPlatform.Services.Audit.Controllers;

/// <summary>
/// Audit trail and compliance endpoints.
/// HIPAA-compliant immutable audit logging.
/// </summary>
[ApiController]
[Route("api/v1/audit")]
[Authorize]
public class AuditController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuditController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Record audit entry.
    /// Called by all services via Kafka listener or direct API.
    /// </summary>
    [HttpPost("entries")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RecordAuditEntry(
        [FromBody] RecordAuditEntryCommand command,
        CancellationToken cancellationToken)
    {
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Record data change.
    /// Before/after tracking for compliance.
    /// </summary>
    [HttpPost("data-changes")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> RecordDataChange(
        [FromBody] RecordDataChangeCommand command,
        CancellationToken cancellationToken)
    {
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Get audit trail for resource (cached).
    /// All actions on this resource in chronological order.
    /// </summary>
    [HttpGet("resources/{resourceType}/{resourceId}")]
    [ProducesResponseType(typeof(AuditTrailResponseDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetResourceAuditTrail(
        string resourceType,
        Guid resourceId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(
            new GetResourceAuditTrailQuery
            {
                ResourceType = resourceType,
                ResourceId = resourceId,
                PageNumber = page,
                PageSize = pageSize
            },
            cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Get user activity (cached).
    /// All actions performed by user with summary.
    /// </summary>
    [HttpGet("users/{userId}/activity")]
    [ProducesResponseType(typeof(UserAuditActivityDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUserAuditActivity(
        Guid userId,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(
            new GetUserAuditActivityQuery
            {
                UserId = userId,
                FromDate = from,
                ToDate = to,
                PageNumber = page,
                PageSize = pageSize
            },
            cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Get compliance reports (cached).
    /// Periodic summaries for audits and regulatory requirements.
    /// </summary>
    [HttpGet("compliance-reports")]
    [ProducesResponseType(typeof(List<ComplianceReportDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetComplianceReports(
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new GetComplianceReportsQuery { FromDate = from, ToDate = to },
            cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Generate compliance report.
    /// Create audit summary for given period.
    /// </summary>
    [HttpPost("compliance-reports/generate")]
    [ProducesResponseType(typeof(ComplianceReportResponseDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> GenerateComplianceReport(
        [FromBody] GenerateComplianceReportCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetComplianceReports), result);
    }

    /// <summary>
    /// Export audit logs.
    /// Generate immutable export for compliance/archive.
    /// </summary>
    [HttpPost("exports")]
    [ProducesResponseType(typeof(AuditExportResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ExportAuditLogs(
        [FromBody] ExportAuditLogsCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(ExportAuditLogs), result);
    }

    /// <summary>
    /// Health check.
    /// </summary>
    [HttpGet("health")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Health()
    {
        return Ok(new { status = "healthy", service = "audit-service" });
    }
}
