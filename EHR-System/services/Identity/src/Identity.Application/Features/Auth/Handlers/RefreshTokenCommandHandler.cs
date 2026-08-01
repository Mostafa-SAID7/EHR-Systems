#nullable enable

using EHRPlatform.BuildingBlocks.Common.Application.CQRS;
using EHRPlatform.BuildingBlocks.Common.Data;
using EHRPlatform.BuildingBlocks.SharedKernel.Exceptions;
using EHRPlatform.BuildingBlocks.Security.Authentication;
using EHRPlatform.Services.Identity.Contracts.Responses;
using EHRPlatform.Services.Identity.Domain.Entities;
using EHRPlatform.Services.Identity.Features.Auth.Commands;
using EHRPlatform.Services.Identity.Infrastructure.Security;
using Microsoft.Extensions.Logging;

namespace EHRPlatform.Services.Identity.Application.Features.Auth.Handlers;

/// <summary>
/// Validates an existing refresh token and issues a new access token.
/// </summary>
public class RefreshTokenCommandHandler : ICommandHandler<RefreshTokenCommand, LoginResponse>
{
    private readonly IUnitOfWork      _uow;
    private readonly IPasswordHasher  _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly ILogger<RefreshTokenCommandHandler> _logger;

    public RefreshTokenCommandHandler(
        IUnitOfWork      uow,
        IPasswordHasher  passwordHasher,
        IJwtTokenService jwtTokenService,
        ILogger<RefreshTokenCommandHandler> logger)
    {
        _uow             = uow             ?? throw new ArgumentNullException(nameof(uow));
        _passwordHasher  = passwordHasher  ?? throw new ArgumentNullException(nameof(passwordHasher));
        _jwtTokenService = jwtTokenService ?? throw new ArgumentNullException(nameof(jwtTokenService));
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

        var newAccessToken = _jwtTokenService.GenerateAccessToken(user);

        _logger.LogInformation("Token refreshed for user: {UserId}", user.Id);

        return new LoginResponse
        {
            AccessToken  = newAccessToken,
            RefreshToken = request.RefreshToken, // return same refresh token
            ExpiresIn    = _jwtTokenService.ExpiresInSeconds,
            MfaRequired  = false
        };
    }
}




