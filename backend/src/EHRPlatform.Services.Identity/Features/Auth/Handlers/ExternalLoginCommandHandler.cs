#nullable enable

using EHRPlatform.Common.Application.CQRS;
using EHRPlatform.Common.Data.Abstractions;
using EHRPlatform.Common.Infrastructure.Security;
using EHRPlatform.Services.Identity.Application.Identity.DTOs.Responses;
using EHRPlatform.Services.Identity.Domain.Entities;
using EHRPlatform.Services.Identity.Features.Auth.Commands;
using EHRPlatform.Services.Identity.Security;
using Microsoft.Extensions.Logging;

namespace EHRPlatform.Services.Identity.Features.Auth.Handlers;

/// <summary>
/// Handler for external OAuth logins (Google, Facebook).
/// Single Responsibility: Verify or provision external user accounts and issue JWT tokens.
/// </summary>
public class ExternalLoginCommandHandler : ICommandHandler<ExternalLoginCommand, LoginResponse>
{
    private readonly IUnitOfWork _uow;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly ILogger<ExternalLoginCommandHandler> _logger;

    public ExternalLoginCommandHandler(
        IUnitOfWork uow,
        IPasswordHasher passwordHasher,
        IJwtTokenService jwtTokenService,
        ILogger<ExternalLoginCommandHandler> logger)
    {
        _uow = uow ?? throw new ArgumentNullException(nameof(uow));
        _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
        _jwtTokenService = jwtTokenService ?? throw new ArgumentNullException(nameof(jwtTokenService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<LoginResponse> Handle(ExternalLoginCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Processing external login via {Provider} for {Email}", request.Provider, request.Email);

        var userRepo = _uow.Repository<User>();
        var user = await userRepo.FirstOrDefaultAsync(
            q => q.Where(u => u.Email == request.Email),
            cancellationToken);

        if (user == null)
        {
            // Auto-provision new user account from OAuth identity
            var (dummyHash, dummySalt) = _passwordHasher.HashWithSalt(Guid.NewGuid().ToString("N"));
            user = new User
            {
                Email = request.Email,
                FirstName = string.IsNullOrWhiteSpace(request.FirstName) ? "OAuth" : request.FirstName,
                LastName = string.IsNullOrWhiteSpace(request.LastName) ? "User" : request.LastName,
                PasswordHash = dummyHash,
                PasswordSalt = dummySalt,
                IsActive = true,
                EmailConfirmed = true,
                MfaEnabled = false
            };

            await userRepo.AddAsync(user, cancellationToken);
            await _uow.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Provisioned new OAuth user account for {Email}", request.Email);
        }

        if (!user.IsActive)
            throw new InvalidOperationException("User account is inactive.");

        var accessToken = _jwtTokenService.GenerateAccessToken(user);
        var refreshToken = Convert.ToBase64String(System.Security.Cryptography.RandomNumberGenerator.GetBytes(32));

        var refreshTokenEntity = new RefreshToken
        {
            UserId = user.Id,
            Token = _passwordHasher.Hash(refreshToken, string.Empty),
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            CreatedBy = user.Id
        };
        await _uow.Repository<RefreshToken>().AddAsync(refreshTokenEntity, cancellationToken);

        user.LastLogin = DateTime.UtcNow;
        user.FailedLoginAttempts = 0;
        await _uow.SaveChangesAsync(cancellationToken);

        return new LoginResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresIn = _jwtTokenService.ExpiresInSeconds,
            MfaRequired = false
        };
    }
}

