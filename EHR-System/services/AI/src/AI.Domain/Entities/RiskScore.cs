namespace EHRPlatform.Services.AI.Domain.Entities;

/// <summary>
/// RiskScore aggregate root - Represents AI-computed patient risk assessment.
/// Includes multiple risk dimensions: readmission, mortality, infection, etc.
/// </summary>
public class RiskScore
{
    public Guid Id { get; set; }
    public Guid PatientId { get; set; }
    public Guid? EncounterId { get; set; }
    public string ModelVersion { get; set; } = string.Empty; // v1.0, v1.1, etc.
    
    // Risk scores (0-100)
    public decimal ReadmissionRisk { get; set; }
    public decimal MortalityRisk { get; set; }
    public decimal InfectionRisk { get; set; }
    public decimal ChronicDiseaseRisk { get; set; }
    public decimal ComplicationRisk { get; set; }
    
    // Composite score
    public decimal OverallRisk { get; set; }
    
    // Risk level
    public string RiskLevel { get; set; } = "Low"; // Low, Medium, High, Critical
    
    // Contributing factors
    public string? TopRiskFactors { get; set; } // JSON array of top 5 factors
    public int FeatureCount { get; set; } // Number of features used
    
    // Model performance metrics
    public decimal ModelConfidence { get; set; } // 0-1
    public string? ModelName { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public ICollection<AnomalyDetection> Anomalies { get; } = new List<AnomalyDetection>();
    public ICollection<RiskPrediction> Predictions { get; } = new List<RiskPrediction>();

    private readonly List<object> _domainEvents = new();

    public void CalculateOverallRisk()
    {
        OverallRisk = (ReadmissionRisk + MortalityRisk + InfectionRisk + ChronicDiseaseRisk + ComplicationRisk) / 5;
        
        RiskLevel = OverallRisk switch
        {
            < 20 => "Low",
            < 40 => "Medium",
            < 70 => "High",
            _ => "Critical"
        };

        UpdatedAt = DateTime.UtcNow;
        RaiseEvent(new RiskScoreCalculatedEvent(Id, PatientId, OverallRisk, RiskLevel));
    }

    public void RaiseEvent(object @event) => _domainEvents.Add(@event);
    public IReadOnlyList<object> GetDomainEvents() => _domainEvents.AsReadOnly();
    public void ClearDomainEvents() => _domainEvents.Clear();
}

/// <summary>
/// AnomalyDetection - Detected anomalies in patient data or patterns
/// </summary>
public class AnomalyDetection
{
    public Guid Id { get; set; }
    public Guid RiskScoreId { get; set; }
    public Guid PatientId { get; set; }
    public string AnomalyType { get; set; } = string.Empty; // Vital, Lab, Medication, Behavior
    public string Description { get; set; } = string.Empty;
    public decimal AnomalyScore { get; set; } // 0-1: deviation from normal
    public bool IsAlerted { get; set; }
    public string? DataPoint { get; set; } // JSON representation of anomalous data
    public DateTime DetectedAt { get; set; }
    public DateTime CreatedAt { get; set; }

    public RiskScore RiskScore { get; set; } = null!;
}

/// <summary>
/// RiskPrediction - Future risk predictions (e.g., will patient be readmitted in 30 days?)
/// </summary>
public class RiskPrediction
{
    public Guid Id { get; set; }
    public Guid RiskScoreId { get; set; }
    public string PredictionType { get; set; } = string.Empty; // Readmission7d, Readmission30d, Mortality90d
    public decimal Probability { get; set; } // 0-1
    public DateTime PredictionWindow { get; set; } // Target date for prediction
    public string? RecommendedActions { get; set; } // JSON array of recommended interventions
    public bool IsAccurate { get; set; } // Set after outcome known
    public DateTime CreatedAt { get; set; }
    public DateTime? VerifiedAt { get; set; }

    public RiskScore RiskScore { get; set; } = null!;
}

/// <summary>
/// MLModel - Registered ML models for scoring
/// </summary>
public class MLModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string ModelType { get; set; } = string.Empty; // XGBoost, RandomForest, NeuralNetwork, Ensemble
    public string S3ModelPath { get; set; } = string.Empty; // Location of serialized model
    public decimal Accuracy { get; set; } // Training accuracy
    public decimal AUC { get; set; } // Area Under ROC Curve
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ActivatedAt { get; set; }
}

// Domain Events
public record RiskScoreCalculatedEvent(Guid RiskScoreId, Guid PatientId, decimal OverallRisk, string RiskLevel)
{
    public DateTime CreatedAt { get; } = DateTime.UtcNow;
}

public record AnomalyDetectedEvent(Guid RiskScoreId, Guid PatientId, string AnomalyType, decimal AnomalyScore)
{
    public DateTime CreatedAt { get; } = DateTime.UtcNow;
}

public record HighRiskAlertEvent(Guid PatientId, decimal RiskScore, string RiskLevel)
{
    public DateTime CreatedAt { get; } = DateTime.UtcNow;
}
