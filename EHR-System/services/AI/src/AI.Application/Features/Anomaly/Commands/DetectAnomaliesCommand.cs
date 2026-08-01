namespace EHRPlatform.Services.AI.Application.Features.Anomaly.Commands;

using MediatR;

/// <summary>
/// Command to detect anomalies in patient data.
/// Uses unsupervised learning to identify unusual patterns in vitals, labs, etc.
/// </summary>
public class DetectAnomaliesCommand : IRequest<DetectAnomaliesResponse>
{
    public Guid PatientId { get; set; }
    public Guid RiskScoreId { get; set; }
    public string DataType { get; set; } = string.Empty; // Vital, Lab, Medication, Behavior
    public Dictionary<string, object> PatientData { get; set; } = new();
}

public class DetectAnomaliesResponse
{
    public Guid PatientId { get; set; }
    public List<DetectedAnomalyDto> Anomalies { get; set; } = new();
    public int AnomalyCount { get; set; }
    public bool HasCriticalAnomaly { get; set; }
}

public class DetectedAnomalyDto
{
    public string AnomalyType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal AnomalyScore { get; set; } // 0-1: how unusual
    public bool RequiresAlert { get; set; }
    public string? RecommendedAction { get; set; }
}
