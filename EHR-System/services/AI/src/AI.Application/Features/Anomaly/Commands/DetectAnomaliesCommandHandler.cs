namespace EHRPlatform.Services.AI.Application.Features.Anomaly.Commands;

using MediatR;
using EHRPlatform.Services.AI.Domain.Entities;
using EHRPlatform.Services.AI.Persistence;
using EHRPlatform.Services.AI.Application.Services;
using Microsoft.Extensions.Logging;

/// <summary>
/// Handler for DetectAnomaliesCommand - Detects anomalies in patient data.
/// </summary>
public class DetectAnomaliesCommandHandler : IRequestHandler<DetectAnomaliesCommand, DetectAnomaliesResponse>
{
    private readonly IAIDbContext _context;
    private readonly IAnomalyDetectionService _anomalyDetectionService;
    private readonly ILogger<DetectAnomaliesCommandHandler> _logger;

    public DetectAnomaliesCommandHandler(
        IAIDbContext context,
        IAnomalyDetectionService anomalyDetectionService,
        ILogger<DetectAnomaliesCommandHandler> logger)
    {
        _context = context;
        _anomalyDetectionService = anomalyDetectionService;
        _logger = logger;
    }

    public async Task<DetectAnomaliesResponse> Handle(DetectAnomaliesCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Detecting anomalies for patient {PatientId} ({DataType})", 
            request.PatientId, request.DataType);

        // Get risk score
        var riskScore = await _context.RiskScores.FindAsync(new object[] { request.RiskScoreId }, cancellationToken);
        if (riskScore == null)
        {
            throw new InvalidOperationException($"Risk score {request.RiskScoreId} not found");
        }

        // Detect anomalies using ML service
        var detectionResult = await _anomalyDetectionService.DetectAsync(
            request.PatientId,
            request.DataType,
            request.PatientData,
            cancellationToken);

        // Store detected anomalies
        var anomalies = new List<AnomalyDetection>();
        var hasCriticalAnomaly = false;

        foreach (var anomaly in detectionResult.DetectedAnomalies)
        {
            var anomalyEntity = new AnomalyDetection
            {
                Id = Guid.NewGuid(),
                RiskScoreId = riskScore.Id,
                PatientId = request.PatientId,
                AnomalyType = anomaly.AnomalyType,
                Description = anomaly.Description,
                AnomalyScore = anomaly.AnomalyScore,
                IsAlerted = anomaly.RequiresAlert,
                DataPoint = System.Text.Json.JsonSerializer.Serialize(anomaly),
                DetectedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };

            _context.AnomalyDetections.Add(anomalyEntity);
            anomalies.Add(anomalyEntity);

            if (anomaly.RequiresAlert)
            {
                hasCriticalAnomaly = true;
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Detected {Count} anomalies for patient {PatientId}. Critical: {IsCritical}", 
            anomalies.Count, request.PatientId, hasCriticalAnomaly);

        return new DetectAnomaliesResponse
        {
            PatientId = request.PatientId,
            Anomalies = detectionResult.DetectedAnomalies,
            AnomalyCount = detectionResult.DetectedAnomalies.Count,
            HasCriticalAnomaly = hasCriticalAnomaly
        };
    }
}
