#nullable enable

using EHRPlatform.BuildingBlocks.Common.Application.CQRS;
using EHRPlatform.BuildingBlocks.Common.Data.Abstractions;
using EHRPlatform.BuildingBlocks.Security.Authentication;
using EHRPlatform.BuildingBlocks.Security.Jwt;
using EHRPlatform.Services.Identity.Contracts.Responses;
using EHRPlatform.Services.Identity.Domain.Entities;
using EHRPlatform.Services.Identity.Features.Auth.Commands;
using Microsoft.Extensions.Logging;

namespace EHRPlatform.Services.Identity.Application.Features.Auth.Handlers;

/// <summary>
/// Handler for external OAuth logins (Google, Facebook).
/// Single Responsibility: Verify or provision external user accounts and issue JWT tokens.
/// Uses building-blocks JWT provider for token generation.
/// </summary>
public class ExternalLoginCommandHandler : ICommandHandler<ExternalLoginCommand, LoginResponse>
{
    private readonly IUnitOfWork _uow;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenProvider _jwtTokenProvider;
    private readonly ILogger<ExternalLoginCommandHandler> _logger;

    public ExternalLoginCommandHandler(
        IUnitOfWork uow,
        IPasswordHasher passwordHasher,
        IJwtTokenProvider jwtTokenProvider,
        ILogger<ExternalLoginCommandHandler> logger)
    {
        _uow = uow ?? throw new ArgumentNullException(nameof(uow));
        _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
        _jwtTokenProvider = jwtTokenProvider ?? throw new ArgumentNullException(nameof(jwtTokenProvider));
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

        // Generate tokens using building-blocks JWT provider
        var roles = user.UserRoles?.Select(ur => ur.Role?.Name ?? "User").ToList() ?? new List<string> { "User" };
        var accessToken = _jwtTokenProvider.GenerateAccessToken(user.Id.ToString(), user.FirstName, user.Email, roles);
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
            ExpiresIn = 3600, // 1 hour
            MfaRequired = false
        };
    }
}
