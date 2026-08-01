namespace EHRPlatform.Services.Audit.API.Controllers;

using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using EHRPlatform.Services.Audit.Application.Features.Audit.Commands;
using EHRPlatform.Services.Audit.Application.Features.Audit.Queries;

/// <summary>
/// Audit API endpoints - Read-only access to audit logs (admin only)
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
    /// Get audit trail for specific resource
    /// </summary>
    [HttpGet("resource/{resourceType}/{resourceId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetResourceAuditTrail(string resourceType, Guid resourceId, [FromQuery] int page = 1, [FromQuery] int size = 50)
    {
        var result = await _mediator.Send(new GetResourceAuditTrailQuery
        {
            ResourceType = resourceType,
            ResourceId = resourceId,
            PageNumber = page,
            PageSize = size
        });
        return Ok(result);
    }

    /// <summary>
    /// Get audit activity for specific user
    /// </summary>
    [HttpGet("user/{userId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUserAuditActivity(
        Guid userId,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] int page = 1,
        [FromQuery] int size = 50)
    {
        var result = await _mediator.Send(new GetUserAuditActivityQuery
        {
            UserId = userId,
            FromDate = from,
            ToDate = to,
            PageNumber = page,
            PageSize = size
        });
        return Ok(result);
    }

    /// <summary>
    /// Health check endpoint
    /// </summary>
    [HttpGet("health")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Health()
    {
        return Ok(new { status = "healthy", service = "AuditService", timestamp = DateTime.UtcNow });
    }
}
