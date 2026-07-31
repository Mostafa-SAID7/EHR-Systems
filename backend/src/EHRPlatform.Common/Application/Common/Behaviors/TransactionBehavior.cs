#nullable enable

namespace EHRPlatform.Common.Application.Common.Behaviors;

using MediatR;
using Microsoft.Extensions.Logging;
using EHRPlatform.Common.Application.Common.CQRS;
using EHRPlatform.Common.Data.Abstractions;

/// <summary>
/// MediatR pipeline behavior for managing database transactions.
/// Wraps command handlers in a transaction to ensure atomicity.
/// Rolls back on any exception, commits on success.
/// </summary>
/// <typeparam name="TRequest">The request type.</typeparam>
/// <typeparam name="TResponse">The response type.</typeparam>
public class TransactionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<TransactionBehavior<TRequest, TResponse>> _logger;

    public TransactionBehavior(
        IUnitOfWork unitOfWork,
        ILogger<TransactionBehavior<TRequest, TResponse>> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        // Only apply transactions to commands, not queries
        if (request is not ICommand)
        {
            return await next();
        }

        var requestName = typeof(TRequest).Name;

        try
        {
            // Begin transaction
            await _unitOfWork.BeginTransactionAsync(cancellationToken);
            _logger.LogDebug("Transaction started for {RequestName}", requestName);

            // Execute handler
            var response = await next();

            // Commit transaction
            await _unitOfWork.CommitTransactionAsync(cancellationToken);
            _logger.LogDebug("Transaction committed for {RequestName}", requestName);

            return response;
        }
        catch (Exception ex)
        {
            // Rollback transaction
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            _logger.LogError(
                ex,
                "Transaction rolled back for {RequestName} due to exception: {ErrorMessage}",
                requestName,
                ex.Message);

            throw;
        }
    }
}

