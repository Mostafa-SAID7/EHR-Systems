namespace EHRPlatform.Services.AI.API.Controllers;

using MediatR;
using EHRPlatform.Services.AI.Application.Features.Risk.Commands;
using EHRPlatform.Services.AI.Application.Features.Risk.Queries;
using EHRPlatform.Services.AI.Application.Features.Anomaly.Commands;
using EHRPlatform.Services.AI.Application.Features.Anomaly.Queries;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.ComponentModel.DataAnnotations;

/// <summary>
/// API endpoints for AI-driven predictions and anomaly detection.
/// Provides risk scoring, anomaly detection, and clinical recommendations.
/// </summary>
[ApiController]
[Route("api/v1/ai")]
[Authorize]
public class PredictionsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<PredictionsController> _logger;

    public PredictionsController(IMediator mediator, ILogger<PredictionsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Compute risk scores for a patient.
    /// POST /api/v1/ai/predict-risk
    /// </summary>
    [HttpPost("predict-risk")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> PredictRisk(
        [FromBody] PredictRiskRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Computing risk scores for patient {PatientId}", request.PatientId);

        var command = new PredictRiskCommand
        {
            PatientId = request.PatientId,
            EncounterId = request.EncounterId,
            PatientFeatures = request.PatientFeatures
        };

        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Get risk score details.
    /// GET /api/v1/ai/risk-scores/{riskScoreId}
    /// </summary>
    [HttpGet("risk-scores/{riskScoreId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetRiskScore(
        [FromRoute] Guid riskScoreId,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting risk score {RiskScoreId}", riskScoreId);

        var query = new GetRiskScoreQuery { RiskScoreId = riskScoreId };
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Detect anomalies in patient data.
    /// POST /api/v1/ai/detect-anomalies
    /// </summary>
    [HttpPost("detect-anomalies")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DetectAnomalies(
        [FromBody] DetectAnomaliesRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Detecting anomalies for patient {PatientId}", request.PatientId);

        var command = new DetectAnomaliesCommand
        {
            PatientId = request.PatientId,
            RiskScoreId = request.RiskScoreId,
            DataType = request.DataType,
            PatientData = request.PatientData ?? new()
        };

        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Get detected anomalies for a patient or risk score.
    /// GET /api/v1/ai/anomalies?riskScoreId=&onlyAlerted=true
    /// </summary>
    [HttpGet("anomalies")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAnomalies(
        [FromQuery] Guid? riskScoreId,
        [FromQuery] Guid? patientId,
        [FromQuery] string? anomalyType,
        [FromQuery] bool onlyAlerted = false,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting anomalies. RiskScore: {RiskScoreId}, Patient: {PatientId}", 
            riskScoreId, patientId);

        var query = new GetAnomaliesQuery
        {
            RiskScoreId = riskScoreId,
            PatientId = patientId,
            AnomalyType = anomalyType,
            OnlyAlerted = onlyAlerted
        };

        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }
}

// Request DTOs
public class PredictRiskRequest
{
    [Required]
    public Guid PatientId { get; set; }
    public Guid? EncounterId { get; set; }
    public Dictionary<string, object>? PatientFeatures { get; set; }
}

public class DetectAnomaliesRequest
{
    [Required]
    public Guid PatientId { get; set; }
    [Required]
    public Guid RiskScoreId { get; set; }
    [Required]
    public string DataType { get; set; } = string.Empty;
    public Dictionary<string, object>? PatientData { get; set; }
}
