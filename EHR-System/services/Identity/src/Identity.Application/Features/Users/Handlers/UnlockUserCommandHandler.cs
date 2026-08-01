#nullable enable

using EHRPlatform.BuildingBlocks.Common.Application.CQRS;
using EHRPlatform.BuildingBlocks.Common.Data.Abstractions;
using EHRPlatform.BuildingBlocks.SharedKernel.Exceptions;
using EHRPlatform.Services.Identity.Domain.Entities;
using EHRPlatform.Services.Identity.Features.Users.Commands;
using Microsoft.Extensions.Logging;

namespace EHRPlatform.Services.Identity.Application.Features.Users.Handlers;

/// <summary>
/// Unlock user command handler.
/// Single Responsibility: Reset failed login count and clear account lockout.
/// </summary>
public class UnlockUserCommandHandler : ICommandHandler<UnlockUserCommand>
{
    private readonly IUnitOfWork _uow;
    private readonly ILogger<UnlockUserCommandHandler> _logger;

    public UnlockUserCommandHandler(IUnitOfWork uow, ILogger<UnlockUserCommandHandler> logger)
    {
        _uow = uow ?? throw new ArgumentNullException(nameof(uow));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task Handle(UnlockUserCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Unlocking user {UserId} by admin {AdminId}", command.UserId, command.UnlockedBy);

        var repo = _uow.Repository<User>();
        var user = await repo.GetByIdAsync(command.UserId, cancellationToken)
            ?? throw new NotFoundException(nameof(User), command.UserId);

        user.Unlock();
        user.UpdatedBy = command.UnlockedBy;

        await repo.UpdateAsync(user, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("User {UserId} successfully unlocked", command.UserId);
    }
}



