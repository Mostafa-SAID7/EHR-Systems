#nullable enable

using EHRPlatform.BuildingBlocks.Common.Application.CQRS;
using EHRPlatform.BuildingBlocks.Common.Data.Abstractions;
using EHRPlatform.BuildingBlocks.SharedKernel.Exceptions;
using EHRPlatform.Services.Identity.Domain.Entities;
using EHRPlatform.Services.Identity.Features.Users.Commands;
using Microsoft.Extensions.Logging;

namespace EHRPlatform.Services.Identity.Features.Users.Handlers;

/// <summary>
/// Delete/Deactivate user command handler.
/// Single Responsibility: Soft delete or deactivate user account by ID.
/// </summary>
public class DeleteUserCommandHandler : ICommandHandler<DeleteUserCommand>
{
    private readonly IUnitOfWork _uow;
    private readonly ILogger<DeleteUserCommandHandler> _logger;

    public DeleteUserCommandHandler(IUnitOfWork uow, ILogger<DeleteUserCommandHandler> logger)
    {
        _uow = uow ?? throw new ArgumentNullException(nameof(uow));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task Handle(DeleteUserCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deactivating user {UserId} by admin {AdminId}", command.UserId, command.DeletedBy);

        var repo = _uow.Repository<User>();
        var user = await repo.GetByIdAsync(command.UserId, cancellationToken)
            ?? throw new NotFoundException(nameof(User), command.UserId);

        user.IsActive = false;
        user.UpdatedBy = command.DeletedBy;
        user.UpdatedAt = DateTime.UtcNow;

        await repo.UpdateAsync(user, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("User {UserId} successfully deactivated", command.UserId);
    }
}


