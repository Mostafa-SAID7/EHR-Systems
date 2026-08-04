namespace EHRPlatform.Services.Analytics.Domain.ValueObjects;

using System.Text.Json;

/// <summary>
/// Value object representing a structured report query definition
/// Encapsulates metric filters, aggregations, and transformations as JSON
/// </summary>
public class ReportQuery : IEquatable<ReportQuery>
{
    /// <summary>
    /// Query definition as JSON
    /// </summary>
    public string QueryJson { get; private set; }

    /// <summary>
    /// Creates new ReportQuery from JSON string
    /// </summary>
    /// <exception cref="JsonException">Thrown if JSON is invalid</exception>
    public ReportQuery(string queryJson)
    {
        if (string.IsNullOrWhiteSpace(queryJson))
        {
            throw new ArgumentException("Query definition cannot be empty");
        }

        try
        {
            JsonDocument.Parse(queryJson);
        }
        catch (JsonException ex)
        {
            throw new ArgumentException("Invalid JSON query definition", nameof(queryJson), ex);
        }

        QueryJson = queryJson;
    }

    /// <summary>
    /// Gets query as JsonDocument
    /// </summary>
    public JsonDocument GetJsonDocument() => JsonDocument.Parse(QueryJson);

    /// <summary>
    /// Gets metrics selected in query
    /// </summary>
    public List<string> GetSelectedMetrics()
    {
        using var doc = GetJsonDocument();
        var metrics = new List<string>();

        if (doc.RootElement.TryGetProperty("metrics", out var metricsArray))
        {
            foreach (var metric in metricsArray.EnumerateArray())
            {
                if (metric.ValueKind == JsonValueKind.String)
                {
                    metrics.Add(metric.GetString() ?? string.Empty);
                }
            }
        }

        return metrics;
    }

    /// <summary>
    /// Gets filter conditions
    /// </summary>
    public JsonElement? GetFilters()
    {
        using var doc = GetJsonDocument();
        if (doc.RootElement.TryGetProperty("filters", out var filters))
        {
            return filters;
        }
        return null;
    }

    /// <summary>
    /// Gets grouping dimensions
    /// </summary>
    public List<string> GetGroupBy()
    {
        using var doc = GetJsonDocument();
        var groupBy = new List<string>();

        if (doc.RootElement.TryGetProperty("groupBy", out var groupByArray))
        {
            foreach (var dimension in groupByArray.EnumerateArray())
            {
                if (dimension.ValueKind == JsonValueKind.String)
                {
                    groupBy.Add(dimension.GetString() ?? string.Empty);
                }
            }
        }

        return groupBy;
    }

    public bool Equals(ReportQuery? other)
    {
        if (other is null) return false;
        return QueryJson == other.QueryJson;
    }

    public override bool Equals(object? obj) => Equals(obj as ReportQuery);

    public override int GetHashCode() => QueryJson.GetHashCode();

    public override string ToString() => QueryJson;
}
