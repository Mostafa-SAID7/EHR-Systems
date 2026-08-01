namespace EHRPlatform.Services.AI.Application.Features.Anomaly.Queries;

using MediatR;

/// <summary>
/// Query to retrieve detected anomalies for a patient or risk score.
/// </summary>
public class GetAnomaliesQuery : IRequest<GetAnomaliesResponse>
{
    public Guid? RiskScoreId { get; set; }
    public Guid? PatientId { get; set; }
    public string? AnomalyType { get; set; }
    public bool OnlyAlerted { get; set; } = false;
}

public class GetAnomaliesResponse
{
    public List<AnomalyDto> Anomalies { get; set; } = new();
    public int TotalCount { get; set; }
    public int AlertedCount { get; set; }
}

public class AnomalyDto
{
    public Guid AnomalyId { get; set; }
    public string AnomalyType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal AnomalyScore { get; set; }
    public bool IsAlerted { get; set; }
    public DateTime DetectedAt { get; set; }
}
