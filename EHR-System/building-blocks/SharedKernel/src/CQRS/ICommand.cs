using EHRPlatform.SharedKernel.Domain;

namespace EHRPlatform.SharedKernel.CQRS;

/// <summary>
/// Marker interface for commands (write operations).
/// Single responsibility: Command contract for mutation operations.
/// </summary>
public interface ICommand
{
}
