#nullable enable

using EHRPlatform.BuildingBlocks.Common.Application.CQRS;
using EHRPlatform.BuildingBlocks.Common.Data;
using EHRPlatform.BuildingBlocks.SharedKernel.Exceptions;
using EHRPlatform.BuildingBlocks.Security.Authentication;
using EHRPlatform.BuildingBlocks.Security.Jwt;
using EHRPlatform.Services.Identity.Contracts.Responses;
using EHRPlatform.Services.Identity.Domain.Entities;
using EHRPlatform.Services.Identity.Features.Auth.Commands;
using Microsoft.Extensions.Logging;

namespace EHRPlatform.Services.Identity.Application.Features.Auth.Handlers;

/// <summary>
/// Validates an existing refresh token and issues a new access token.
/// Uses building-blocks JWT provider for token generation.
/// </summary>
public class RefreshTokenCommandHandler : ICommandHandler<RefreshTokenCommand, LoginResponse>
{
    private readonly IUnitOfWork      _uow;
    private readonly IPasswordHasher  _passwordHasher;
    private readonly IJwtTokenProvider _jwtTokenProvider;
    private readonly ILogger<RefreshTokenCommandHandler> _logger;

    public RefreshTokenCommandHandler(
        IUnitOfWork      uow,
        IPasswordHasher  passwordHasher,
        IJwtTokenProvider jwtTokenProvider,
        ILogger<RefreshTokenCommandHandler> logger)
    {
        _uow             = uow             ?? throw new ArgumentNullException(nameof(uow));
        _passwordHasher  = passwordHasher  ?? throw new ArgumentNullException(nameof(passwordHasher));
        _jwtTokenProvider = jwtTokenProvider ?? throw new ArgumentNullException(nameof(jwtTokenProvider));
        _logger          = logger          ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<LoginResponse> Handle(
        RefreshTokenCommand request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Token refresh request received");

        // Hash the incoming token the same way it was stored
        var hashedToken = _passwordHasher.Hash(request.RefreshToken, string.Empty);

        var rtRepo = _uow.Repository<RefreshToken>();
        var refreshTokenEntity = await rtRepo.FirstOrDefaultAsync(
            q => q.Where(rt => rt.Token == hashedToken && rt.ExpiresAt > DateTime.UtcNow),
            cancellationToken)
            ?? throw new UnauthorizedException("Invalid or expired refresh token");

        var user = await _uow.Repository<User>().GetByIdAsync(refreshTokenEntity.UserId, cancellationToken)
            ?? throw new NotFoundException(nameof(User), refreshTokenEntity.UserId);

        if (!user.IsActive)
            throw new UnauthorizedException("User account is inactive");

        // Generate new access token using building-blocks JWT provider
        var roles = user.UserRoles?.Select(ur => ur.Role?.Name ?? "User").ToList() ?? new List<string> { "User" };
        var newAccessToken = _jwtTokenProvider.GenerateAccessToken(user.Id.ToString(), user.FirstName, user.Email, roles);

        _logger.LogInformation("Token refreshed for user: {UserId}", user.Id);

        return new LoginResponse
        {
            AccessToken  = newAccessToken,
            RefreshToken = request.RefreshToken, // return same refresh token
            ExpiresIn    = 3600, // 1 hour
            MfaRequired  = false
        };
    }
}
