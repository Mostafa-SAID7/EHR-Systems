namespace EHRPlatform.SharedKernel.CQRS;

/// <summary>
/// Typed command returning a result.
/// Single responsibility: Typed command contract with return value.
/// </summary>
public interface ICommand<out TResult> : ICommand
{
}
