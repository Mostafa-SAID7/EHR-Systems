#nullable enable

using System.Text;

namespace EHRPlatform.Common.Domain.Events;

/// <summary>
/// Extension methods for integration event string processing.
/// </summary>
public static class IntegrationEventExtensions
{
    /// <summary>
    /// Converts PascalCase to kebab-case for Kafka topic naming.
    /// Example: "PatientCreatedEvent" → "patient-created-event"
    /// </summary>
    /// <param name="input">PascalCase string to convert</param>
    /// <returns>kebab-case string</returns>
    public static string ToKebabCase(this string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        var result = new StringBuilder();
        for (int i = 0; i < input.Length; i++)
        {
            if (char.IsUpper(input[i]) && i > 0)
                result.Append('-');
            result.Append(char.ToLowerInvariant(input[i]));
        }

        return result.ToString();
    }
}
