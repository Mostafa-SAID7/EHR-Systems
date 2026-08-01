using System.Collections.Generic;

namespace EHRPlatform.Common.Validation;

/// <summary>
/// Validation result containing errors.
/// Single responsibility: Validation result data structure.
/// </summary>
public class ValidationResult
{
    /// <summary>
    /// Is validation successful.
    /// </summary>
    public bool IsValid { get; set; } = true;

    /// <summary>
    /// Validation errors by property.
    /// </summary>
    public Dictionary<string, List<string>> Errors { get; set; } = new();

    /// <summary>
    /// Add error for a property.
    /// </summary>
    public void AddError(string property, string message)
    {
        if (!Errors.ContainsKey(property))
            Errors[property] = new List<string>();

        Errors[property].Add(message);
        IsValid = false;
    }

    /// <summary>
    /// Get all errors as flat list.
    /// </summary>
    public List<string> GetAllErrors()
    {
        var all = new List<string>();
        foreach (var kvp in Errors)
            all.AddRange(kvp.Value);
        return all;
    }
}
