#nullable enable

using EHRPlatform.Common.Application.CQRS;

namespace EHRPlatform.Services.Identity.Features.Users.Commands;

/// <summary>
/// Command to unlock a locked user account.
/// Single Responsibility: Encapsulate payload for clearing account lockout.
/// </summary>
public record UnlockUserCommand(Guid UserId, Guid UnlockedBy) : ICommand;

