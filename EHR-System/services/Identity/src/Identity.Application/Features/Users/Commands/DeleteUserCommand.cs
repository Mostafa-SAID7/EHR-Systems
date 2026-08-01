#nullable enable

using EHRPlatform.BuildingBlocks.Common.Application.CQRS;

namespace EHRPlatform.Services.Identity.Application.Features.Users.Commands;

/// <summary>
/// Command to delete/deactivate a user.
/// Single Responsibility: Encapsulate payload required for deactivating user accounts.
/// </summary>
public record DeleteUserCommand(Guid UserId, Guid DeletedBy) : ICommand;



