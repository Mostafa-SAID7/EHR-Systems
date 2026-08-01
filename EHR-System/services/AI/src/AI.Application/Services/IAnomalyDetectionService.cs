namespace EHRPlatform.Services.AI.Application.Services;

using EHRPlatform.Services.AI.Application.Features.Anomaly.Commands;

/// <summary>
/// Interface for anomaly detection service.
/// Detects anomalies in patient data using unsupervised learning.
/// </summary>
public interface IAnomalyDetectionService
{
    /// <summary>
    /// Detects anomalies in patient data.
    /// </summary>
    Task<AnomalyDetectionResult> DetectAsync(
        Guid patientId,
        string dataType,
        Dictionary<string, object> patientData,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets baseline/normal ranges for a patient.
    /// </summary>
    Task<PatientBaseline> GetPatientBaselineAsync(
        Guid patientId,
        string dataType,
        CancellationToken cancellationToken = default);
}

public class AnomalyDetectionResult
{
    public List<DetectedAnomalyDto> DetectedAnomalies { get; set; } = new();
    public decimal OverallAnomalyScore { get; set; }
}

public class PatientBaseline
{
    public Guid PatientId { get; set; }
    public string DataType { get; set; } = string.Empty;
    public Dictionary<string, (double Mean, double Std)> NormalRanges { get; set; } = new();
    public DateTime LastUpdated { get; set; }
}
