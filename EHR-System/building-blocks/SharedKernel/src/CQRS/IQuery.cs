namespace EHRPlatform.SharedKernel.CQRS;

/// <summary>
/// Marker interface for queries (read operations).
/// Single responsibility: Query contract for read-only operations.
/// </summary>
public interface IQuery<out TResult>
{
}
