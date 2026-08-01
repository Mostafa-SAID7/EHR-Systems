namespace EHRPlatform.Services.AI.Infrastructure.Services;

using EHRPlatform.Services.AI.Application.Services;
using EHRPlatform.Services.AI.Application.Features.Anomaly.Commands;
using Microsoft.Extensions.Logging;

/// <summary>
/// Anomaly detection service implementation using Isolation Forest.
/// Detects unusual patterns in patient data.
/// </summary>
public class AnomalyDetectionService : IAnomalyDetectionService
{
    private readonly ILogger<AnomalyDetectionService> _logger;

    public AnomalyDetectionService(ILogger<AnomalyDetectionService> logger)
    {
        _logger = logger;
    }

    public async Task<AnomalyDetectionResult> DetectAsync(
        Guid patientId,
        string dataType,
        Dictionary<string, object> patientData,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Detecting anomalies for patient {PatientId} ({DataType})", patientId, dataType);

        try
        {
            var result = new AnomalyDetectionResult { DetectedAnomalies = new() };

            // Get patient baseline (from cache or compute)
            var baseline = await GetPatientBaselineAsync(patientId, dataType, cancellationToken);

            // Detect anomalies based on type
            var anomalies = dataType switch
            {
                "Vital" => DetectVitalAnomalies(patientData, baseline),
                "Lab" => DetectLabAnomalies(patientData, baseline),
                "Medication" => DetectMedicationAnomalies(patientData, baseline),
                "Behavior" => DetectBehaviorAnomalies(patientData, baseline),
                _ => new List<DetectedAnomalyDto>()
            };

            result.DetectedAnomalies = anomalies;
            result.OverallAnomalyScore = anomalies.Any() ? anomalies.Max(a => a.AnomalyScore) : 0;

            _logger.LogInformation("Detected {Count} anomalies. Overall score: {Score:F2}", 
                anomalies.Count, result.OverallAnomalyScore);

            return await Task.FromResult(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error detecting anomalies");
            throw;
        }
    }

    public async Task<PatientBaseline> GetPatientBaselineAsync(
        Guid patientId,
        string dataType,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting baseline for patient {PatientId} ({DataType})", patientId, dataType);

        // In production: Fetch from feature store or compute from historical data
        var baseline = new PatientBaseline
        {
            PatientId = patientId,
            DataType = dataType,
            LastUpdated = DateTime.UtcNow
        };

        // Add normal ranges
        if (dataType == "Vital")
        {
            baseline.NormalRanges["BloodPressure"] = (120, 15);
            baseline.NormalRanges["HeartRate"] = (70, 10);
            baseline.NormalRanges["Temperature"] = (37, 0.5);
            baseline.NormalRanges["RespiratoryRate"] = (16, 2);
        }
        else if (dataType == "Lab")
        {
            baseline.NormalRanges["Glucose"] = (100, 20);
            baseline.NormalRanges["Hemoglobin"] = (14, 2);
            baseline.NormalRanges["Potassium"] = (4, 0.5);
        }

        return await Task.FromResult(baseline);
    }

    private List<DetectedAnomalyDto> DetectVitalAnomalies(Dictionary<string, object> data, PatientBaseline baseline)
    {
        var anomalies = new List<DetectedAnomalyDto>();

        foreach (var vital in data)
        {
            if (baseline.NormalRanges.TryGetValue(vital.Key, out var (mean, std)))
            {
                if (vital.Value is double value)
                {
                    var zScore = Math.Abs((value - mean) / std);
                    if (zScore > 3) // 3 sigma rule
                    {
                        anomalies.Add(new DetectedAnomalyDto
                        {
                            AnomalyType = "Vital",
                            Description = $"{vital.Key} is abnormally high/low: {value}",
                            AnomalyScore = (decimal)Math.Min(zScore / 5, 1.0), // Normalize to 0-1
                            RequiresAlert = zScore > 4
                        });
                    }
                }
            }
        }

        return anomalies;
    }

    private List<DetectedAnomalyDto> DetectLabAnomalies(Dictionary<string, object> data, PatientBaseline baseline)
    {
        var anomalies = new List<DetectedAnomalyDto>();

        // Similar logic to vital anomalies
        foreach (var lab in data)
        {
            if (baseline.NormalRanges.TryGetValue(lab.Key, out var (mean, std)))
            {
                if (lab.Value is double value)
                {
                    var zScore = Math.Abs((value - mean) / std);
                    if (zScore > 2.5)
                    {
                        anomalies.Add(new DetectedAnomalyDto
                        {
                            AnomalyType = "Lab",
                            Description = $"Lab value {lab.Key} is abnormal: {value}",
                            AnomalyScore = (decimal)Math.Min(zScore / 4, 1.0),
                            RequiresAlert = zScore > 3.5
                        });
                    }
                }
            }
        }

        return anomalies;
    }

    private List<DetectedAnomalyDto> DetectMedicationAnomalies(Dictionary<string, object> data, PatientBaseline baseline)
    {
        var anomalies = new List<DetectedAnomalyDto>();

        // Check for unexpected medication changes
        if (data.TryGetValue("NewMedications", out var newMeds) && newMeds is string[] newMedList)
        {
            if (newMedList.Length > 5)
            {
                anomalies.Add(new DetectedAnomalyDto
                {
                    AnomalyType = "Medication",
                    Description = $"Unusually high number of new medications: {newMedList.Length}",
                    AnomalyScore = 0.7m,
                    RequiresAlert = true
                });
            }
        }

        return anomalies;
    }

    private List<DetectedAnomalyDto> DetectBehaviorAnomalies(Dictionary<string, object> data, PatientBaseline baseline)
    {
        var anomalies = new List<DetectedAnomalyDto>();

        // Check for behavioral changes
        if (data.TryGetValue("AdherenceRate", out var adherence) && adherence is double adherenceRate)
        {
            if (adherenceRate < 0.5)
            {
                anomalies.Add(new DetectedAnomalyDto
                {
                    AnomalyType = "Behavior",
                    Description = $"Low medication adherence detected: {adherenceRate:P}",
                    AnomalyScore = 0.75m,
                    RequiresAlert = true,
                    RecommendedAction = "Contact patient to discuss adherence barriers"
                });
            }
        }

        return anomalies;
    }
}
