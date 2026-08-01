namespace EHRPlatform.BuildingBlocks.Contracts.Responses;

/// <summary>
/// Standardized API error response for all services.
/// Used by global exception middleware for consistent error handling.
/// </summary>
public class ApiErrorResponse
{
    public string TraceId { get; set; } = string.Empty;
    public int StatusCode { get; set; }
    public string Message { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public Dictionary<string, string[]>? Errors { get; set; }
    public string? Details { get; set; }
}
