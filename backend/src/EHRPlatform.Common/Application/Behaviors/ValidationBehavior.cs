#nullable enable

namespace EHRPlatform.Common.Application.Behaviors;

using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

/// <summary>
/// MediatR pipeline behavior for validating commands and queries.
/// Runs all registered FluentValidation validators before the handler executes.
/// </summary>
/// <typeparam name="TRequest">The request type to validate.</typeparam>
/// <typeparam name="TResponse">The response type.</typeparam>
public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;
    private readonly ILogger<ValidationBehavior<TRequest, TResponse>> _logger;

    public ValidationBehavior(
        IEnumerable<IValidator<TRequest>> validators,
        ILogger<ValidationBehavior<TRequest, TResponse>> logger)
    {
        _validators = validators;
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var context = new ValidationContext<TRequest>(request);
        var failures = new List<FluentValidation.Results.ValidationFailure>();

        // Run all validators
        foreach (var validator in _validators)
        {
            var result = await validator.ValidateAsync(context, cancellationToken);
            if (!result.IsValid)
            {
                failures.AddRange(result.Errors);
            }
        }

        // If validation failed, throw exception
        if (failures.Any())
        {
            _logger.LogWarning(
                "Validation failed for {RequestType}: {Failures}",
                typeof(TRequest).Name,
                string.Join(", ", failures.Select(f => $"{f.PropertyName}: {f.ErrorMessage}")));

            throw new ValidationException(
                $"Validation failed for {typeof(TRequest).Name}",
                failures);
        }

        // Validation passed, proceed to next handler
        return await next();
    }
}

