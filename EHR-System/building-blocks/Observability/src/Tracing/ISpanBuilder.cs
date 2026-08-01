using System;

namespace EHRPlatform.Observability.Tracing;

/// <summary>
/// Span builder for fluent span creation.
/// Single responsibility: Fluent API for span configuration.
/// </summary>
public interface ISpanBuilder
{
    /// <summary>
    /// Set span attribute.
    /// </summary>
    ISpanBuilder SetAttribute(string key, object value);

    /// <summary>
    /// Add event to span.
    /// </summary>
    ISpanBuilder AddEvent(string name, DateTime? timestamp = null);

    /// <summary>
    /// Set span tag.
    /// </summary>
    ISpanBuilder SetTag(string key, string value);

    /// <summary>
    /// Build and start span.
    /// </summary>
    ISpan Build();
}
